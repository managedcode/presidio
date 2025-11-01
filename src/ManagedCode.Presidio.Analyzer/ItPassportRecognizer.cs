namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Italian passport numbers using the canonical regex.
/// </summary>
public sealed class ItPassportRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "it",
    string supportedEntity = "IT_PASSPORT") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Passport (very weak)", @"(?i)\b[A-Z]{2}\d{7}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "passaporto",
        "elettronico",
        "italiano",
        "viaggio",
        "viaggiare",
        "estero",
        "documento",
        "dogana",
    };
}
