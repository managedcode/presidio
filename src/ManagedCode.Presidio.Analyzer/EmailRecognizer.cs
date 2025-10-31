using System.Globalization;
using System.Text.RegularExpressions;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes email addresses using regex and simple domain validation.
/// Mirrors python <c>EmailRecognizer</c>.
/// </summary>
public sealed class EmailRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "EMAIL_ADDRESS")
    : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private const string EmailRegex =
        @"\b((([!#$%&'*+\-/=?^_`{|}~\w])|([!#$%&'*+\-/=?^_`{|}~\w][!#$%&'*+\-/=?^_`{|}~\.\w-]{0,}[!#$%&'*+\-/=?^_`{|}~\w]))[@][\w-]+([-.][\w-]+)*\.[\w-]+([-.][\w-]+)*)\b";

    private static readonly Pattern[] DefaultPatterns =
    {
        new("Email (Medium)", EmailRegex, 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "email",
    };

    private static readonly Regex InvalidDomainCharacters = new("[\\s@]", RegexOptions.Compiled);

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var atIndex = patternText.LastIndexOf('@');
        if (atIndex <= 0 || atIndex >= patternText.Length - 1)
        {
            return false;
        }

        var domain = patternText[(atIndex + 1)..];
        if (InvalidDomainCharacters.IsMatch(domain))
        {
            return false;
        }

        // Attempt to convert Unicode domains to ASCII to align with tldextract behaviour.
        try
        {
            var idn = new IdnMapping();
            domain = idn.GetAscii(domain);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (!domain.Contains('.', StringComparison.Ordinal))
        {
            return false;
        }

        var labels = domain.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (labels.Length < 2)
        {
            return false;
        }

        foreach (var label in labels)
        {
            if (!IsValidDomainLabel(label))
            {
                return false;
            }
        }

        var tld = labels[^1];
        if (tld.Length < 2)
        {
            return false;
        }

        if (!IsValidTopLevelDomain(tld))
        {
            return false;
        }

        return true;
    }

    private static bool IsValidDomainLabel(string label)
    {
        if (label.Length == 0 || label.Length > 63)
        {
            return false;
        }

        if (label.StartsWith('-') || label.EndsWith('-'))
        {
            return false;
        }

        foreach (var ch in label)
        {
            if (!(char.IsLetterOrDigit(ch) || ch == '-'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidTopLevelDomain(string tld)
    {
        if (tld.StartsWith("xn--", StringComparison.OrdinalIgnoreCase))
        {
            // Allow punycode TLDs with alphanumeric payload.
            var suffix = tld[4..];
            return suffix.Length >= 1 && suffix.All(ch => char.IsLetterOrDigit(ch) || ch == '-');
        }

        return tld.All(char.IsLetter);
    }
}
