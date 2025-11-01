namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes US bank account numbers using Presidio's canonical patterns and context hints.
/// </summary>
public sealed class UsBankRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "US_BANK_NUMBER") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("Bank Account (weak)", @"\b[0-9]{8,17}\b", 0.05),
    };

    private static readonly string[] DefaultContext =
    {
        "check",
        "account",
        "account#",
        "acct",
        "bank",
        "save",
        "debit",
    };
}

