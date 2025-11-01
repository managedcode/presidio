namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian passport numbers using canonical regex.
/// </summary>
public sealed class InPassportRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_PASSPORT") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("PASSPORT", @"\b[A-Z][1-9]\d\s?\d{4}[1-9]\b", 0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "passport",
        "indian passport",
        "passport number",
    };
}

