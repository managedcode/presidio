namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian Voter ID (EPIC) numbers using canonical patterns.
/// </summary>
public sealed class InVoterRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_VOTER") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("VOTER", @"\b([A-Za-z]{1}[ABCDGHJKMNPRSYabcdghjkmnprsy]{1}[A-Za-z]{1}([0-9]){7})\b", 0.4),
        new("VOTER Alt", @"\b([A-Za-z]){3}([0-9]){7}\b", 0.3),
    };

    private static readonly string[] DefaultContext =
    {
        "voter",
        "epic",
        "elector photo identity card",
    };
}

