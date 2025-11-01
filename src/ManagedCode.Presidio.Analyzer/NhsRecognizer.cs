namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes UK NHS numbers with checksum validation parity.
/// </summary>
public sealed class NhsRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "UK_NHS",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("NHS (medium)", @"\b([0-9]{3})[- ]?([0-9]{3})[- ]?([0-9]{4})\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "national health service",
        "nhs",
        "health services authority",
        "health authority",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new[]
        {
            ("-", string.Empty),
            (" ", string.Empty),
        };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (sanitized.Length != 10 || sanitized.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        var total = 0;
        for (var i = 0; i < sanitized.Length; i++)
        {
            total += (sanitized[i] - '0') * (10 - i);
        }

        return total % 11 == 0;
    }
}
