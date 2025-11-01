namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Australian Company Numbers (ACN) with checksum validation.
/// </summary>
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
    {
        new("ACN (Medium)", @"\b\d{3}\s\d{3}\s\d{3}\b", 0.1),
        new("ACN (Low)", @"\b\d{9}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "australian company number",
        "acn",
    };

    private static readonly int[] Weights = { 8, 7, 6, 5, 4, 3, 2, 1 };

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
        if (sanitized.Length != 9 || sanitized.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        Span<int> digits = stackalloc int[9];
        for (var i = 0; i < sanitized.Length; i++)
        {
            digits[i] = sanitized[i] - '0';
        }

        var sum = 0;
        for (var i = 0; i < Weights.Length; i++)
        {
            sum += digits[i] * Weights[i];
        }

        var remainder = sum % 10;
        var checkDigit = (10 - remainder) % 10;
        return checkDigit == digits[^1];
    }
}
