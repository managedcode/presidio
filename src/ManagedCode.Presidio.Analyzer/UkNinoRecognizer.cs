namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes UK National Insurance numbers using Presidio's canonical regex and context.
/// </summary>
public sealed class UkNinoRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "UK_NINO") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "NINO (medium)",
            @"\b(?!bg|gb|nk|kn|nt|tn|zz|BG|GB|NK|KN|NT|TN|ZZ) ?([a-ceghj-pr-tw-zA-CEGHJ-PR-TW-Z]{1}[a-ceghj-npr-tw-zA-CEGHJ-NPR-TW-Z]{1}) ?([0-9]{2}) ?([0-9]{2}) ?([0-9]{2}) ?([a-dA-D{1}])\b",
            0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "national insurance",
        "ni number",
        "nino",
    };
}

