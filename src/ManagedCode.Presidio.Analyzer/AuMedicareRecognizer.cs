namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Australian Medicare numbers using checksum validation.
/// </summary>
public sealed class AuMedicareRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "AU_MEDICARE",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Medicare (Medium)", @"\b\d{4}\s\d{5}\s\d\b", 0.1),
        new("Medicare (Low)", @"\b\d{10}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "medicare",
        "australian medicare",
    };

    private static readonly int[] Weights = { 1, 3, 7, 9, 1, 3, 7, 9 };

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

        Span<int> digits = stackalloc int[10];
        for (var i = 0; i < sanitized.Length; i++)
        {
            digits[i] = sanitized[i] - '0';
        }

        var sum = 0;
        for (var i = 0; i < Weights.Length; i++)
        {
            sum += digits[i] * Weights[i];
        }

        return (sum % 10) == digits[8];
    }
}
