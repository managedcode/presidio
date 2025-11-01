using System.Globalization;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Finnish personal identity codes (Henkilötunnus) and validates control characters.
/// </summary>
public sealed class FiPersonalIdentityCodeRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "fi",
    string supportedEntity = "FI_PERSONAL_IDENTITY_CODE") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "Finnish Personal Identity Code (Medium)",
            @"\b(\d{6})([+\-ABCDEFYXWVU])(\d{3})([0123456789ABCDEFHJKLMNPRSTUVWXY])\b",
            0.5),
        new(
            "Finnish Personal Identity Code (Very Weak)",
            @"(\d{6})([+\-ABCDEFYXWVU])(\d{3})([0123456789ABCDEFHJKLMNPRSTUVWXY])",
            0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "hetu",
        "henkil\u00F6tunnus",
        "personbeteckningen",
        "personal identity code",
    };

    private const string ControlCharacters = "0123456789ABCDEFHJKLMNPRSTUVWXY";

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText) || patternText.Length != 11)
        {
            return false;
        }

        var datePart = patternText[..6];
        if (!DateTime.TryParseExact(datePart, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }

        var individualPart = patternText[7..10];
        if (!int.TryParse(datePart + individualPart, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            return false;
        }

        var controlCharacter = patternText[^1];
        var expected = ControlCharacters[number % ControlCharacters.Length];
        return expected == controlCharacter;
    }
}
