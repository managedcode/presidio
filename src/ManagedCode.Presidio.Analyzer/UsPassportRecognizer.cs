namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes US passport numbers using canonical Presidio patterns and context hints.
/// </summary>
public sealed class UsPassportRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "US_PASSPORT") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Passport (very weak)", @"\b[0-9]{9}\b", 0.05),
        new("Passport Next Generation (very weak)", @"\b[A-Z][0-9]{8}\b", 0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "us",
        "united",
        "states",
        "passport",
        "passport#",
        "travel",
        "document",
    };
}

