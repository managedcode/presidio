namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Spanish NIE numbers with checksum validation.
/// </summary>
public sealed class EsNieRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "es",
    string supportedEntity = "ES_NIE",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("NIE", "\\b[X-Z]?[0-9]?[0-9]{7}[-]?[A-Z]\\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "número de identificación de extranjero",
        "nie",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new (string, string)[] { ("-", string.Empty), (" ", string.Empty) };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText.ToUpperInvariant(), _replacementPairs);

        if (sanitized.Length < 8 || sanitized.Length > 9)
        {
            return false;
        }

        if (!"XYZ".Contains(sanitized[0]))
        {
            return false;
        }

        var numericPart = sanitized.Substring(1, sanitized.Length - 2);
        if (!numericPart.All(char.IsDigit))
        {
            return false;
        }

        var letters = "TRWAGMYFPDXBNJZSQVHLCKE";
        var letter = sanitized[^1];
        var number = int.Parse("XYZ".IndexOf(sanitized[0]).ToString() + numericPart);
        return letter == letters[number % 23];
    }
}
