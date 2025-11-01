namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian PAN identifiers using canonical regex patterns.
/// </summary>
public sealed class InPanRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_PAN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("PAN (High)", @"\b([A-Za-z]{3}[AaBbCcFfGgHhJjLlPpTt]{1}[A-Za-z]{1}[0-9]{4}[A-Za-z]{1})\b", 0.5),
        new("PAN (Medium)", @"\b([A-Za-z]{5}[0-9]{4}[A-Za-z]{1})\b", 0.1),
        new("PAN (Low)", @"\b((?=.*?[a-zA-Z])(?=.*?[0-9]{4})[\w@#$%^?~-]{10})\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "permanent account number",
        "pan",
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
        return sanitized.Length == 10;
    }
}

