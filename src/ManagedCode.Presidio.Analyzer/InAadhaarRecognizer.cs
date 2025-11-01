using System.Globalization;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian Aadhaar numbers using regex, Verhoeff checksum, and palindrome validation.
/// </summary>
public sealed class InAadhaarRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_AADHAAR",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("AADHAAR (Very Weak)", @"\b[0-9]{12}\b", 0.01),
        new("AADHAAR (Grouped)", @"\b[0-9]{4}[- :][0-9]{4}[- :][0-9]{4}\b", 0.01),
    };

    private static readonly string[] DefaultContext =
    {
        "aadhaar",
        "uidai",
    };

    private static readonly int[,] VerhoeffD =
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
        {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
        {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
        {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
        {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
        {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
        {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
        {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
        {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
    };

    private static readonly int[,] VerhoeffP =
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
        {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
        {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
        {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
        {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
        {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
        {7, 0, 4, 6, 9, 1, 3, 2, 5, 8},
    };

    private static readonly int[] VerhoeffInverse = { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new[]
        {
            ("-", string.Empty),
            (" ", string.Empty),
            (":", string.Empty),
        };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        return CheckAadhaar(sanitized);
    }

    private static bool CheckAadhaar(string value)
    {
        if (value.Length != 12 || value.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        if (value[0] < '2')
        {
            return false;
        }

        if (IsPalindrome(value, caseInsensitive: false))
        {
            return false;
        }

        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            return false;
        }

        return IsVerhoeffNumber(number);
    }

    internal static bool IsPalindrome(string text, bool caseInsensitive = false)
    {
        if (text is null)
        {
            return false;
        }

        var candidate = caseInsensitive ? text.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant() : text;
        var span = candidate.AsSpan();
        for (int i = 0, j = span.Length - 1; i < j; i++, j--)
        {
            if (span[i] != span[j])
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsVerhoeffNumber(long inputNumber)
    {
        var digits = inputNumber.ToString(CultureInfo.InvariantCulture)
            .Reverse()
            .Select(ch => ch - '0')
            .ToArray();

        var c = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            c = VerhoeffD[c, VerhoeffP[i % 8, digits[i]]];
        }

        return VerhoeffInverse[c] == 0;
    }
}

