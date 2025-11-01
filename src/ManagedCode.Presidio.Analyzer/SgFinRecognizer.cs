namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Singapore FIN/NRIC identifiers using regex patterns.
/// </summary>
public sealed class SgFinRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "SG_NRIC_FIN") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Nric (weak)", @"(?i)(\b[A-Z][0-9]{7}[A-Z]\b)", 0.3),
        new("Nric (medium)", @"(?i)(\b[STFGM][0-9]{7}[A-Z]\b)", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "fin",
        "fin#",
        "nric",
        "nric#",
    };
}
