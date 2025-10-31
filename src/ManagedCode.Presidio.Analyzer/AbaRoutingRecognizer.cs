namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes U.S. ABA routing numbers using regex and checksum validation.
/// </summary>
public sealed class AbaRoutingRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "ABA_ROUTING_NUMBER",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("ABA routing number (weak)", "\\b[0123678]\\d{8}\\b", 0.05),
        new("ABA routing number", "\\b[0123678]\\d{3}-\\d{4}-\\d\\b", 0.3),
    };

    private static readonly string[] DefaultContext =
    {
        "aba",
        "routing",
        "abarouting",
        "association",
        "bankrouting",
    };

    private static readonly (string Search, string Replacement)[] DefaultReplacementPairs =
    {
        ("-", string.Empty),
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs = replacementPairs?.ToArray() ?? DefaultReplacementPairs;

    protected override bool? ValidateResult(string patternText)
    {
        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        return Checksum(sanitized);
    }

    private static bool Checksum(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 9 || !value.All(char.IsDigit))
        {
            return false;
        }

        var multipliers = new[] { 3, 7, 1, 3, 7, 1, 3, 7, 1 };
        var sum = 0;
        for (var i = 0; i < multipliers.Length; i++)
        {
            sum += (value[i] - '0') * multipliers[i];
        }

        return sum % 10 == 0;
    }
}
