namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Korean resident registration numbers with checksum validation.
/// </summary>
public sealed class KrRrnRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "ko",
    string supportedEntity = "KR_RRN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("RRN (Medium)", "\\b\\d{2}(0[1-9]|1[0-2])(0[1-9]|[1-2][0-9]|3[0-1])(-?)\\d{7}\\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "Korean RRN",
        "Korean Resident Registration Number",
        "Resident Registration Number",
        "RRN",
        "rrn",
        "rrn#",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new (string, string)[] { ("-", string.Empty) };

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

        var regionCode = int.Parse(sanitized.Substring(7, 2));
        if (regionCode < 0 || regionCode > 95)
        {
            return null;
        }

        return ValidateChecksum(sanitized) ? true : null;
    }

    private static bool ValidateChecksum(string rrn)
    {
        Span<int> digits = stackalloc int[13];
        for (var i = 0; i < rrn.Length; i++)
        {
            digits[i] = rrn[i] - '0';
        }

        var digitSum =
            2 * digits[0] + 3 * digits[1] + 4 * digits[2] + 5 * digits[3] + 6 * digits[4] + 7 * digits[5] +
            8 * digits[6] + 9 * digits[7] + 2 * digits[8] + 3 * digits[9] + 4 * digits[10] + 5 * digits[11];

        var checksum = (11 - (digitSum % 11)) % 10;
        return checksum == digits[12];
    }
}
