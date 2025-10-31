namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes credit card numbers using regex patterns and a Luhn checksum.
/// </summary>
public sealed class CreditCardRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "CREDIT_CARD",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "All Credit Cards (weak)",
            @"\b(?!1\d{12}(?!\d))((4\d{3})|(5[0-5]\d{2})|(6\d{3})|(1\d{3})|(3\d{3}))[- ]?(\d{3,4})[- ]?(\d{3,4})[- ]?(\d{3,5})\b",
            0.3),
    };

    private static readonly string[] DefaultContext =
    {
        "credit",
        "card",
        "visa",
        "mastercard",
        "cc ",
        "amex",
        "discover",
        "jcb",
        "diners",
        "maestro",
        "instapayment",
    };

    private static readonly (string Search, string Replacement)[] DefaultReplacementPairs =
    {
        ("-", string.Empty),
        (" ", string.Empty),
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs = replacementPairs?.ToArray() ?? DefaultReplacementPairs;

    protected override bool? ValidateResult(string patternText)
    {
        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        return LuhnChecksum(sanitized);
    }

    private static bool LuhnChecksum(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        var digits = value.Select(ch => ch - '0').ToArray();
        var sum = 0;
        var alternate = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var current = digits[i];
            if (alternate)
            {
                current *= 2;
                if (current > 9)
                {
                    current -= 9;
                }
            }

            sum += current;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}
