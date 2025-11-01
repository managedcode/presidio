namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Australian Tax File Numbers (TFN) with checksum validation.
/// </summary>
public sealed class AuTfnRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "AU_TFN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("TFN (Medium)", @"\b\d{3}\s\d{3}\s\d{3}\b", 0.1),
        new("TFN (Low)", @"\b\d{9}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "tax file number",
        "tfn",
    };

    private static readonly int[] Weights = { 1, 4, 3, 7, 5, 8, 6, 9, 10 };

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

        return sum % 11 == 0;
    }
}
