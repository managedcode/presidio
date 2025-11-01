namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Polish PESEL numbers and applies checksum validation.
/// </summary>
public sealed class PlPeselRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "pl",
    string supportedEntity = "PL_PESEL") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "PESEL",
            @"[0-9]{2}([02468][1-9]|[13579][012])(0[1-9]|1[0-9]|2[0-9]|3[01])[0-9]{5}",
            0.4),
    };

    private static readonly string[] DefaultContext =
    {
        "pesel",
    };

    private static readonly int[] Weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText) || patternText.Length != 11)
        {
            return false;
        }

        Span<int> digits = stackalloc int[11];
        for (var i = 0; i < patternText.Length; i++)
        {
            var ch = patternText[i];
            if (!char.IsDigit(ch))
            {
                return false;
            }

            digits[i] = ch - '0';
        }

        var checksum = 0;
        for (var i = 0; i < 10; i++)
        {
            checksum += digits[i] * Weights[i];
        }

        checksum %= 10;
        return checksum == digits[10];
    }
}
