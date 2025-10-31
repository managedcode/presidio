namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes dates using regex patterns. Mirrors the Python date recognizer.
/// </summary>
public sealed class DateRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "DATE_TIME")
    : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("ISO 8601 datetime", @"\b(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+([+-][0-2]\d:[0-5]\d|Z))|(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d([+-][0-2]\d:[0-5]\d|Z))|(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d([+-][0-2]\d:[0-5]\d|Z))\b", 0.8),
        new("mm/dd/yyyy or mm/dd/yy", @"\b(([1-9]|0[1-9]|1[0-2])/([1-9]|0[1-9]|[1-2][0-9]|3[0-1])/(\d{4}|\d{2}))\b", 0.6),
        new("dd/mm/yyyy or dd/mm/yy", @"\b(([1-9]|0[1-9]|[1-2][0-9]|3[0-1])/([1-9]|0[1-9]|1[0-2])/(\d{4}|\d{2}))\b", 0.6),
        new("yyyy/mm/dd", @"\b(\d{4}/([1-9]|0[1-9]|1[0-2])/([1-9]|0[1-9]|[1-2][0-9]|3[0-1]))\b", 0.6),
        new("mm-dd-yyyy", @"\b(([1-9]|0[1-9]|1[0-2])-([1-9]|0[1-9]|[1-2][0-9]|3[0-1])-\d{4})\b", 0.6),
        new("dd-mm-yyyy", @"\b(([1-9]|0[1-9]|[1-2][0-9]|3[0-1])-([1-9]|0[1-9]|1[0-2])-\d{4})\b", 0.6),
        new("yyyy-mm-dd", @"\b(\d{4}-([1-9]|0[1-9]|1[0-2])-([1-9]|0[1-9]|[1-2][0-9]|3[0-1]))\b", 0.6),
        new("dd.mm.yyyy or dd.mm.yy", @"\b(([1-9]|0[1-9]|[1-2][0-9]|3[0-1])\.([1-9]|0[1-9]|1[0-2])\.(\d{4}|\d{2}))\b", 0.6),
        new("dd-MMM-yyyy or dd-MMM-yy", @"\b(([1-9]|0[1-9]|[1-2][0-9]|3[0-1])-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)-(\d{4}|\d{2}))\b", 0.6),
        new("MMM-yyyy or MMM-yy", @"\b((JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)-(\d{4}|\d{2}))\b", 0.6),
        new("dd-MMM", @"\b(([1-9]|0[1-9]|[1-2][0-9]|3[0-1])-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC))\b", 0.6),
        new("mm/yyyy or m/yyyy", @"\b(([1-9]|0[1-9]|1[0-2])/\d{4})\b", 0.2),
        new("mm/yy or m/yy", @"\b(([1-9]|0[1-9]|1[0-2])/\d{2})\b", 0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "date",
        "birthday",
    };
}
