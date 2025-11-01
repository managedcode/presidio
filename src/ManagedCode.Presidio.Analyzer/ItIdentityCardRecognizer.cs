namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Italian identity card numbers (paper and electronic variants).
/// </summary>
public sealed class ItIdentityCardRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "it",
    string supportedEntity = "IT_IDENTITY_CARD") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Paper-based Identity Card (very weak)", @"(?i)\b[A-Z]{2}\s?\d{7}\b", 0.01),
        new("Electronic Identity Card (CIE) 2.0 (very weak)", @"(?i)\b\d{7}[A-Z]{2}\b", 0.01),
        new("Electronic Identity Card (CIE) 3.0 (very weak)", @"(?i)\b[A-Z]{2}\d{5}[A-Z]{2}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "carta",
        "identità",
        "elettronica",
        "cie",
        "documento",
        "riconoscimento",
        "espatrio",
    };
}
