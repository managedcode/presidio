namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Italian fiscal codes (Codice Fiscale) with checksum validation.
/// </summary>
public sealed class ItFiscalCodeRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "it",
    string supportedEntity = "IT_FISCAL_CODE") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new(
            "Fiscal Code",
            "(?i)((?:[A-Z][AEIOU][AEIOUX]|[AEIOU]X{2}|[B-DF-HJ-NP-TV-Z]{2}[A-Z]){2}(?:[\\dLMNP-V]{2}(?:[A-EHLMPR-T](?:[04LQ][1-9MNP-V]|[15MR][\\dLMNP-V]|[26NS][0-8LMNP-U])|[DHPS][37PT][0L]|[ACELMRT][37PT][01LM]|[AC-EHLMPR-T][26NS][9V])|(?:[02468LNQSU][048LQU]|[13579MPRTV][26NS])B[26NS][9V])(?:[A-MZ][1-9MNP-V][\\dLMNP-V]{2}|[A-M][0L](?:[1-9MNP-V][\\dLMNP-V]|[0L][1-9MNP-V]))[A-Z])",
            0.3),
    };

    private static readonly string[] DefaultContext =
    {
        "codice fiscale",
        "cf",
    };

    private static readonly IReadOnlyDictionary<char, int> OddMap = new Dictionary<char, int>
    {
        ['0'] = 1,
        ['1'] = 0,
        ['2'] = 5,
        ['3'] = 7,
        ['4'] = 9,
        ['5'] = 13,
        ['6'] = 15,
        ['7'] = 17,
        ['8'] = 19,
        ['9'] = 21,
        ['A'] = 1,
        ['B'] = 0,
        ['C'] = 5,
        ['D'] = 7,
        ['E'] = 9,
        ['F'] = 13,
        ['G'] = 15,
        ['H'] = 17,
        ['I'] = 19,
        ['J'] = 21,
        ['K'] = 2,
        ['L'] = 4,
        ['M'] = 18,
        ['N'] = 20,
        ['O'] = 11,
        ['P'] = 3,
        ['Q'] = 6,
        ['R'] = 8,
        ['S'] = 12,
        ['T'] = 14,
        ['U'] = 16,
        ['V'] = 10,
        ['W'] = 22,
        ['X'] = 25,
        ['Y'] = 24,
        ['Z'] = 23,
    };

    private static readonly IReadOnlyDictionary<char, int> EvenMap = Enumerable.Range(0, 10)
        .ToDictionary(i => (char)('0' + i), i => i)
        .Concat(Enumerable.Range(0, 26).ToDictionary(i => (char)('A' + i), i => i))
        .ToDictionary(pair => pair.Key, pair => pair.Value);

    private static readonly IReadOnlyDictionary<int, char> ModMap = Enumerable.Range(0, 26)
        .ToDictionary(i => i, i => (char)('A' + i));

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var upper = patternText.Trim().ToUpperInvariant();
        if (upper.Length != 16)
        {
            return false;
        }

        var control = upper[^1];
        var core = upper[..^1];

        var oddSum = 0;
        var evenSum = 0;
        for (var i = 0; i < core.Length; i++)
        {
            var ch = core[i];
            if (i % 2 == 0)
            {
                if (!OddMap.TryGetValue(ch, out var value))
                {
                    return false;
                }

                oddSum += value;
            }
            else
            {
                if (!EvenMap.TryGetValue(ch, out var value))
                {
                    return false;
                }

                evenSum += value;
            }
        }

        var checkIndex = (oddSum + evenSum) % 26;
        var expected = ModMap[checkIndex];
        return expected == control ? true : null;
    }
}
using System.Collections.Generic;
using System.Linq;
