using System.Text.RegularExpressions;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian GSTIN identifiers with structural validation.
/// </summary>
public sealed class InGstinRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_GSTIN",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("GSTIN (High)", @"\b((?:0[1-9]|[1-3][0-7])[A-Za-z0-9]{10}[A-Za-z0-9]{1}Z[A-Za-z0-9]{1})\b", 0.8),
        new("GSTIN (Medium)", @"\b((?:0[1-9]|[1-3][0-7])[A-Za-z0-9]{11}Z[A-Za-z0-9]{1})\b", 0.4),
        new("GSTIN (Low)", @"\b([0-9]{2}[A-Za-z0-9]{11}Z[A-Za-z0-9]{1})\b", 0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "gstin",
        "gst",
        "goods and services tax",
        "tax identification",
        "gst number",
        "gst registration",
    };

    private static readonly Regex ExtractionRegex = new(
        @"\b((?:0[1-9]|[1-3][0-7])[A-Z]{5}[0-9]{4}[A-Z][0-9A-Z]Z[0-9A-Z])\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

        var sanitized = SanitizeValue(patternText);
        return ValidateGstin(sanitized);
    }

    internal string SanitizeValue(string text)
    {
        var candidate = ExtractionRegex.Match(text.ToUpperInvariant());
        if (candidate.Success)
        {
            return candidate.Groups[1].Value;
        }

        var sanitized = text.ToUpperInvariant();
        foreach (var (search, replacement) in _replacementPairs)
        {
            sanitized = sanitized.Replace(search, replacement, StringComparison.Ordinal);
        }

        return sanitized;
    }

    internal static bool ValidateGstin(string gstin)
    {
        if (gstin.Length != 15)
        {
            return false;
        }

        var stateCode = gstin[..2];
        if (!int.TryParse(stateCode, out var state) || state < 1 || state > 37)
        {
            return false;
        }

        var panPart = gstin.Substring(2, 10);
        if (!ValidatePanFormat(panPart))
        {
            return false;
        }

        var registration = gstin[12];
        if (!char.IsLetterOrDigit(registration))
        {
            return false;
        }

        if (gstin[13] != 'Z')
        {
            return false;
        }

        var checksum = gstin[14];
        return char.IsLetterOrDigit(checksum);
    }

    internal static bool ValidatePanFormat(string pan)
    {
        if (pan.Length != 10)
        {
            return false;
        }

        var firstFive = pan[..5];
        var letterCount = firstFive.Count(char.IsLetter);
        if (letterCount < 3)
        {
            return false;
        }

        if (!pan.Substring(5, 4).All(char.IsDigit))
        {
            return false;
        }

        if (!char.IsLetter(pan[9]))
        {
            return false;
        }

        return true;
    }
}
