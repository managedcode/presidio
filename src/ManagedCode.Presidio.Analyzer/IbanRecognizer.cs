using System.Text;
using System.Text.RegularExpressions;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes IBAN values using regex patterns, per-country formatting rules, and checksum validation.
/// </summary>
public sealed class IbanRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IBAN_CODE",
    bool exactMatch = false,
    (string Bos, string Eos)? bosEos = null,
    RegexOptions? regexOptions = null,
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage,
        globalRegexOptions: regexOptions ?? DefaultRegexOptions)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "IBAN Generic",
            @"\b([A-Z]{2}[ \-]?[0-9]{2})(?=(?:[ \-]?[A-Z0-9]){9,30})((?:[ \-]?[A-Z0-9]{3,5}){2})([ \-]?[A-Z0-9]{3,5})?([ \-]?[A-Z0-9]{3,5})?([ \-]?[A-Z0-9]{3,5})?([ \-]?[A-Z0-9]{3,5})?([ \-]?[A-Z0-9]{3,5})?([ \-]?[A-Z0-9]{1,3})?\b",
            0.5),
    };

    private static readonly string[] DefaultContext = { "iban", "bank", "transaction" };

    private const RegexOptions DefaultRegexOptions =
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase;

    private static readonly (string Search, string Replacement)[] DefaultReplacementPairs =
    {
        ("-", string.Empty),
        (" ", string.Empty),
    };

    private static readonly IReadOnlyDictionary<char, string> Letters = BuildLetterMap();

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs = replacementPairs?.ToArray() ?? DefaultReplacementPairs;
    private readonly bool _exactMatch = exactMatch;
    private readonly (string Bos, string Eos) _bosEos = bosEos ?? (IbanPatterns.BOS, IbanPatterns.EOS);
    private readonly RegexOptions _formatRegexOptions = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline;

    protected override bool? ValidateResult(string patternText)
    {
        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Length < 4)
        {
            return false;
        }

        var expectedDigits = sanitized.Substring(2, 2);
        var generatedDigits = GenerateIbanCheckDigits(sanitized);
        if (!string.Equals(expectedDigits, generatedDigits, StringComparison.Ordinal))
        {
            return false;
        }

        if (!_exactMatch)
        {
            return true;
        }

        if (IsValidFormat(sanitized))
        {
            return true;
        }

        if (IsValidFormat(sanitized.ToUpperInvariant()))
        {
            return null;
        }

        return false;
    }

    private bool IsValidFormat(string iban)
    {
        if (iban.Length < 4)
        {
            return false;
        }

        var countryCode = iban[..2].ToUpperInvariant();
        if (!IbanPatterns.RegexPerCountry.TryGetValue(countryCode, out var regexPattern))
        {
            return false;
        }

        if (_exactMatch)
        {
            regexPattern = _bosEos.Bos + regexPattern + _bosEos.Eos;
        }

        return Regex.IsMatch(iban, regexPattern, _formatRegexOptions);
    }

    private static string GenerateIbanCheckDigits(string iban)
    {
        var transformed = (iban[..2] + "00" + iban[4..]).ToUpperInvariant();
        var numeric = NumberIban(transformed);
        var remainder = Mod97(numeric);
        var checkDigits = 98 - remainder;
        return checkDigits.ToString("D2");
    }

    private static string NumberIban(string iban)
    {
        var swapped = iban[4..] + iban[..4];
        var builder = new StringBuilder(swapped.Length * 2);
        foreach (var ch in swapped)
        {
            if (!Letters.TryGetValue(char.ToUpperInvariant(ch), out var digits))
            {
                throw new ArgumentException($"Unexpected IBAN character '{ch}'.", nameof(iban));
            }

            builder.Append(digits);
        }

        return builder.ToString();
    }

    private static int Mod97(string numeric)
    {
        var remainder = 0;
        foreach (var ch in numeric)
        {
            remainder = (remainder * 10 + (ch - '0')) % 97;
        }

        return remainder;
    }

    private static IReadOnlyDictionary<char, string> BuildLetterMap()
    {
        var map = new Dictionary<char, string>(36);
        for (var i = 0; i < 10; i++)
        {
            map[(char)('0' + i)] = i.ToString();
        }

        for (var i = 0; i < 26; i++)
        {
            map[(char)('A' + i)] = (i + 10).ToString();
        }

        return map;
    }
}
