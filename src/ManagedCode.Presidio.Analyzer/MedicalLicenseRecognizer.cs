namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes US medical (DEA) license numbers with checksum validation parity.
/// </summary>
public sealed class MedicalLicenseRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "MEDICAL_LICENSE",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "USA DEA Certificate Number (weak)",
            @"[abcdefghjklmprstuxABCDEFGHJKLMPRSTUX]{1}[a-zA-Z]{1}\d{7}|[abcdefghjklmprstuxABCDEFGHJKLMPRSTUX]{1}9\d{7}",
            0.4),
    };

    private static readonly string[] DefaultContext =
    {
        "medical",
        "certificate",
        "dea",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new[]
        {
            ("-", string.Empty),
            (" ", string.Empty),
        };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (sanitized.Length < 3)
        {
            return false;
        }

        var digitsSpan = sanitized.AsSpan(2);
        if (digitsSpan.Length < 2)
        {
            return false;
        }

        for (var i = 0; i < digitsSpan.Length; i++)
        {
            if (!char.IsDigit(digitsSpan[i]))
            {
                return false;
            }
        }

        return ValidateChecksum(digitsSpan);
    }

    private static bool ValidateChecksum(ReadOnlySpan<char> digits)
    {
        if (digits.Length == 0)
        {
            return false;
        }

        Span<int> buffer = stackalloc int[digits.Length];
        for (var i = 0; i < digits.Length; i++)
        {
            buffer[i] = digits[i] - '0';
        }

        var total = -buffer[^1];
        var doubleDigit = true;
        for (var index = buffer.Length - 2; index >= 0; index--)
        {
            total += doubleDigit ? buffer[index] * 2 : buffer[index];
            doubleDigit = !doubleDigit;
        }

        return total % 10 == 0;
    }
}
