namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes US Social Security Numbers using Presidio's canonical patterns and validation rules.
/// </summary>
public sealed class UsSsnRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "US_SSN") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("SSN1 (very weak)", @"\b([0-9]{5})-([0-9]{4})\b", 0.05),
        new("SSN2 (very weak)", @"\b([0-9]{3})-([0-9]{6})\b", 0.05),
        new("SSN3 (very weak)", @"\b(([0-9]{3})-([0-9]{2})-([0-9]{4}))\b", 0.05),
        new("SSN4 (very weak)", @"\b[0-9]{9}\b", 0.05),
        new("SSN5 (medium)", @"\b([0-9]{3})[- .]([0-9]{2})[- .]([0-9]{4})\b", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "social",
        "security",
        "ssn",
        "ssns",
        "ssid",
    };

    private static readonly HashSet<string> DisallowedPrefixes = new(StringComparer.Ordinal)
    {
        "000",
        "666",
    };

    private static readonly string[] DisallowedStarts =
    {
        "123456789",
        "98765432",
        "078051120",
    };

    protected override bool? InvalidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return true;
        }

        if (HasMismatchedDelimiters(patternText))
        {
            return true;
        }

        var digits = ExtractDigits(patternText);
        if (digits.Length != 9)
        {
            return true;
        }

        if (AllCharactersEqual(digits))
        {
            return true;
        }

        if (digits.AsSpan(3, 2).SequenceEqual("00".AsSpan()) ||
            digits.AsSpan(5, 4).SequenceEqual("0000".AsSpan()))
        {
            return true;
        }

        if (DisallowedPrefixes.Contains(digits[..3]))
        {
            return true;
        }

        foreach (var sample in DisallowedStarts)
        {
            if (digits.StartsWith(sample, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasMismatchedDelimiters(string value)
    {
        var delimiters = new HashSet<char>();
        foreach (var ch in value)
        {
            if (ch is '.' or '-' or ' ')
            {
                delimiters.Add(ch);
                if (delimiters.Count > 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string ExtractDigits(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;
        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
            {
                buffer[index++] = ch;
            }
        }

        return new string(buffer[..index]);
    }

    private static bool AllCharactersEqual(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var first = value[0];
        for (var i = 1; i < value.Length; i++)
        {
            if (value[i] != first)
            {
                return false;
            }
        }

        return true;
    }
}
