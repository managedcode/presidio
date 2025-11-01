namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Italian driver license numbers using canonical regex patterns.
/// </summary>
public sealed class ItDriverLicenseRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "it",
    string supportedEntity = "IT_DRIVER_LICENSE") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "Driver License",
            @"\b(?i)(([A-Z]{2}\d{7}[A-Z])|(^[U]1[BCDEFGHLJKMNPRSTUWYXZ0-9]{7}[A-Z]))\b",
            0.2),
    };

    private static readonly string[] DefaultContext =
    {
        "patente",
        "patente di guida",
        "licenza",
        "licenza di guida",
    };
}

