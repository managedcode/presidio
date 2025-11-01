namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes US Individual Taxpayer Identification Numbers (ITIN) using Presidio's canonical patterns.
/// </summary>
public sealed class UsItinRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "US_ITIN") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "Itin (very weak)",
            @"\b9\d{2}[- ](5\d|6[0-5]|7\d|8[0-8]|9([0-2]|[4-9]))\d{4}\b|\b9\d{2}(5\d|6[0-5]|7\d|8[0-8]|9([0-2]|[4-9]))[- ]\d{4}\b",
            0.05),
        new(
            "Itin (weak)",
            @"\b9\d{2}(5\d|6[0-5]|7\d|8[0-8]|9([0-2]|[4-9]))\d{4}\b",
            0.3),
        new(
            "Itin (medium)",
            @"\b9\d{2}[- ](5\d|6[0-5]|7\d|8[0-8]|9([0-2]|[4-9]))[- ]\d{4}\b",
            0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "individual",
        "taxpayer",
        "itin",
        "tax",
        "payer",
        "taxid",
        "tin",
    };
}

