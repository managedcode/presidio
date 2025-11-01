namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Thai National ID numbers (TNIN) with checksum validation.
/// </summary>
public sealed class ThTninRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "th",
    string supportedEntity = "TH_TNIN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("TNIN (Medium)", @"\b[1-9](?:[134][0-9]|[25][0134567]|[67][01234567]|[89][0123456])\d{10}\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "Thai National ID",
        "Thai ID Number",
        "TNIN",
        "เลขประจำตัวประชาชน",
        "เลขบัตรประชาชน",
        "รหัสปชช",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? Array.Empty<(string, string)>();

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (sanitized.Length != 13 || !sanitized.All(char.IsDigit))
        {
            return false;
        }

        return ValidateChecksum(sanitized);
    }

    private static bool ValidateChecksum(string tnin)
    {
        Span<int> digits = stackalloc int[13];
        for (var i = 0; i < tnin.Length; i++)
        {
            digits[i] = tnin[i] - '0';
        }

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += (13 - i) * digits[i];
        }

        var x = sum % 11;
        var expected = x <= 1 ? 1 - x : 11 - x;

        return expected == digits[12];
    }
}
