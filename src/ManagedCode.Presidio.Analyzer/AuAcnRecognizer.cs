namespace ManagedCode.Presidio.Analyzer;

/// <summary>Recognizes Australian Company Numbers (ACNs) with the ASIC check digit.</summary>
public sealed class AuAcnRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "AU_ACN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    [
        new("ACN (Medium)", @"\b\d{3}\s\d{3}\s\d{3}\b", 0.1),
        new("ACN (Low)", @"\b\d{9}\b", 0.01),
    ];

    private static readonly string[] DefaultContext = ["australian company number", "acn"];
    private static readonly int[] Weights = [8, 7, 6, 5, 4, 3, 2, 1];

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs = replacementPairs?.ToArray() ??
    [
        ("-", string.Empty),
        (" ", string.Empty),
    ];

    protected override bool? ValidateResult(string patternText)
    {
        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (sanitized.Length != 9 || sanitized.Any(character => character is < '0' or > '9'))
        {
            return false;
        }

        var weightedSum = 0;
        for (var index = 0; index < Weights.Length; index++)
        {
            weightedSum += (sanitized[index] - '0') * Weights[index];
        }

        return (10 - (weightedSum % 10)) % 10 == sanitized[^1] - '0';
    }
}
