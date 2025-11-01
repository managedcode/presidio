namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Australian Business Numbers (ABN) using canonical patterns and checksum validation.
/// </summary>
public sealed class AuAbnRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "AU_ABN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("ABN (Medium)", @"\b\d{2}\s\d{3}\s\d{3}\s\d{3}\b", 0.1),
        new("ABN (Low)", @"\b\d{11}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "australian business number",
        "abn",
    };

    private static readonly int[] Weights = { 10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };

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
        if (sanitized.Length != 11 || sanitized.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        Span<int> digits = stackalloc int[11];
        for (var i = 0; i < sanitized.Length; i++)
        {
            digits[i] = sanitized[i] - '0';
        }

        digits[0] = digits[0] == 0 ? 9 : digits[0] - 1;

        var sum = 0;
        for (var i = 0; i < Weights.Length; i++)
        {
            sum += digits[i] * Weights[i];
        }

        return sum % 89 == 0;
    }
}
