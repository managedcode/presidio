using ManagedCode.Presidio.Core;
using PhoneNumbers;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes multi-regional phone numbers using libphonenumber parity with the Python implementation.
/// </summary>
public sealed class PhoneRecognizer : EntityRecognizer
{
    public const double DefaultScore = 0.4;

    private static readonly string[] DefaultContext =
    {
        "phone",
        "number",
        "telephone",
        "cell",
        "cellphone",
        "mobile",
        "call",
    };

    private static readonly string[] DefaultSupportedRegions =
    {
        "US",
        "UK",
        "DE",
        "FE",
        "IL",
        "IN",
        "CA",
        "BR",
    };

    private readonly IReadOnlyCollection<string> _supportedRegions;
    private readonly PhoneNumberUtil _phoneNumberUtil;
    private readonly PhoneNumberUtil.Leniency _leniency;

    public PhoneRecognizer(
        IEnumerable<string>? context = null,
        string supportedLanguage = "en",
        IEnumerable<string>? supportedRegions = null,
        int? leniency = 1)
        : base(new[] { "PHONE_NUMBER" }, supportedLanguage: supportedLanguage, context: context ?? DefaultContext)
    {
        _phoneNumberUtil = PhoneNumberUtil.GetInstance();
        var supportedRegionSet = _phoneNumberUtil.GetSupportedRegions();

        _supportedRegions = (supportedRegions ?? DefaultSupportedRegions)
            .Select(NormalizeRegionCode)
            .Where(supportedRegionSet.Contains)
            .DefaultIfEmpty("US")
            .ToArray();

        _leniency = ConvertLeniency(leniency);
    }

    protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<RecognizerResult>();
        }

        if (!ShouldAnalyze(entities))
        {
            return Array.Empty<RecognizerResult>();
        }

        var results = new List<RecognizerResult>();
        foreach (var region in _supportedRegions)
        {
            foreach (var match in _phoneNumberUtil.FindNumbers(text, region, _leniency, long.MaxValue))
            {
                var parsedRegion = _phoneNumberUtil.GetRegionCodeForNumber(match.Number);
                var metadataRegion = string.IsNullOrWhiteSpace(parsedRegion) ? region : parsedRegion;

                var explanation = BuildAnalysisExplanation(metadataRegion);
                var metadata = new Dictionary<string, object?>
                {
                    [RecognitionMetadataKeys.RecognizerName] = Name,
                    [RecognitionMetadataKeys.RecognizerIdentifier] = Id,
                    ["phone_region"] = metadataRegion,
                };

                var result = new RecognizerResult(
                    "PHONE_NUMBER",
                    new TextSpan(match.Start, match.Start + match.Length),
                    DefaultScore,
                    explanation,
                    metadata);

                results.Add(result);
            }
        }

        return RemoveDuplicates(results);
    }

    private static string NormalizeRegionCode(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            return "US";
        }

        var normalized = region.Trim().ToUpperInvariant();
        return normalized switch
        {
            "UK" => "GB",
            _ => normalized,
        };
    }

    private static bool ShouldAnalyze(IReadOnlyCollection<string> entities)
    {
        if (entities is null || entities.Count == 0)
        {
            return true;
        }

        return entities.Contains("PHONE_NUMBER", StringComparer.Ordinal);
    }

    private static PhoneNumberUtil.Leniency ConvertLeniency(int? leniency)
    {
        return leniency switch
        {
            null => PhoneNumberUtil.Leniency.VALID,
            <= 0 => PhoneNumberUtil.Leniency.POSSIBLE,
            1 => PhoneNumberUtil.Leniency.VALID,
            2 => PhoneNumberUtil.Leniency.STRICT_GROUPING,
            >= 3 => PhoneNumberUtil.Leniency.EXACT_GROUPING,
        };
    }

    private static AnalysisExplanation BuildAnalysisExplanation(string? region)
    {
        var regionText = string.IsNullOrWhiteSpace(region) ? "unknown" : region;
        var explanation = new AnalysisExplanation(
            nameof(PhoneRecognizer),
            DefaultScore,
            textualExplanation: $"Recognized as {regionText} region phone number, using PhoneRecognizer");
        explanation.SetImprovedScore(DefaultScore);
        return explanation;
    }
}
