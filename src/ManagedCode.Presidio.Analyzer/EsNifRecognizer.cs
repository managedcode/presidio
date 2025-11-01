namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Spanish NIF identifiers with checksum validation.
/// </summary>
public sealed class EsNifRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "es",
    string supportedEntity = "ES_NIF",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("NIF", "\\b[0-9]?[0-9]{7}[-]?[A-Z]\\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "documento nacional de identidad",
        "dni",
        "nif",
        "identificación",
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
        if (sanitized.Length < 2)
        {
            return false;
        }

        var letter = sanitized[^1];
        var digits = new string(sanitized.Take(sanitized.Length - 1).Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return false;
        }

        var number = int.Parse(digits);
        var letters = "TRWAGMYFPDXBNJZSQVHLCKE";
        return letter == letters[number % 23];
    }
}
