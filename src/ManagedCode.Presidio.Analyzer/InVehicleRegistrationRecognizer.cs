using System.Collections.Immutable;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Indian vehicle registration numbers (RTO plates) with regional validation.
/// </summary>
public sealed class InVehicleRegistrationRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "IN_VEHICLE_REGISTRATION",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("India Vehicle Registration (Very Weak)", @"\b[A-Z]{1}(?!0000)[0-9]{4}\b", 0.01),
        new("India Vehicle Registration (Very Weak)", @"\b[A-Z]{2}(?!0000)\d{4}\b", 0.01),
        new("India Vehicle Registration (Very Weak)", @"\b(I)(?!00000)\d{5}\b", 0.01),
        new("India Vehicle Registration (Weak)", @"\b[A-Z]{3}(?!0000)\d{4}\b", 0.20),
        new("India Vehicle Registration (Medium)", @"\b\d{1,3}(CD|CC|UN)[1-9]{1}[0-9]{1,3}\b", 0.40),
        new("India Vehicle Registration", @"\b[A-Z]{2}\d{1}[A-Z]{1,3}(?!0000)\d{4}\b", 0.50),
        new("India Vehicle Registration", @"\b[A-Z]{2}\d{2}[A-Z]{1,2}(?!0000)\d{4}\b", 0.50),
        new("India Vehicle Registration", @"\b[2-9]{1}[1-9]{1}(BH)(?!0000)\d{4}[A-HJ-NP-Z]{2}\b", 0.85),
        new("India Vehicle Registration", @"\b(?!00)\d{2}(A|B|C|D|E|F|H|K|P|R|X)\d{6}[A-Z]{1}\b", 0.85),
    };

    private static readonly string[] DefaultContext =
    {
        "RTO",
        "vehicle",
        "plate",
        "registration",
    };

    // vehicle district sets count 38
    private static readonly string[] IN_VEHICLE_DIST_AN = new[] { "01" };
    private static readonly string[] IN_VEHICLE_DIST_AP = new[] { "39", "40" };
    private static readonly string[] IN_VEHICLE_DIST_AR = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "19", "20", "22" };
    private static readonly string[] IN_VEHICLE_DIST_AS = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34" };
    private static readonly string[] IN_VEHICLE_DIST_BR = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "19", "21", "22", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "37", "38", "39", "43", "44", "45", "46", "50", "51", "52", "53", "55", "56" };
    private static readonly string[] IN_VEHICLE_DIST_CG = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30" };
    private static readonly string[] IN_VEHICLE_DIST_CH = new[] { "01", "02", "03", "04" };
    private static readonly string[] IN_VEHICLE_DIST_DD = new[] { "01", "02", "03" };
    private static readonly string[] IN_VEHICLE_DIST_DL = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };
    private static readonly string[] IN_VEHICLE_DIST_DN = new[] { "09" };
    private static readonly string[] IN_VEHICLE_DIST_GA = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
    private static readonly string[] IN_VEHICLE_DIST_GJ = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39" };
    private static readonly string[] IN_VEHICLE_DIST_HP = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    private static readonly string[] IN_VEHICLE_DIST_HR = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    private static readonly string[] IN_VEHICLE_DIST_JH = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24" };
    private static readonly string[] IN_VEHICLE_DIST_JK = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22" };
    private static readonly string[] IN_VEHICLE_DIST_KA = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71" };
    private static readonly string[] IN_VEHICLE_DIST_KL = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    private static readonly string[] IN_VEHICLE_DIST_LA = new[] { "01", "02" };
    private static readonly string[] IN_VEHICLE_DIST_LD = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09" };
    private static readonly string[] IN_VEHICLE_DIST_MH = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51" };
    private static readonly string[] IN_VEHICLE_DIST_ML = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };
    private static readonly string[] IN_VEHICLE_DIST_MN = new[] { "01", "02", "03", "04", "05", "06", "07" };
    private static readonly string[] IN_VEHICLE_DIST_MP = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71" };
    private static readonly string[] IN_VEHICLE_DIST_MZ = new[] { "01", "02", "03", "04", "05", "06", "07", "08" };
    private static readonly string[] IN_VEHICLE_DIST_NL = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };
    private static readonly string[] IN_VEHICLE_DIST_OD = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35" };
    private static readonly string[] IN_VEHICLE_DIST_OR = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };
    private static readonly string[] IN_VEHICLE_DIST_PB = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    private static readonly string[] IN_VEHICLE_DIST_PY = new[] { "01", "02", "03", "04", "05" };
    private static readonly string[] IN_VEHICLE_DIST_RJ = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58" };
    private static readonly string[] IN_VEHICLE_DIST_SK = new[] { "01", "02", "03", "04", "05", "06", "07", "08" };
    private static readonly string[] IN_VEHICLE_DIST_TN = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    private static readonly string[] IN_VEHICLE_DIST_TR = new[] { "01", "02", "03", "04", "05", "06", "07", "08" };
    private static readonly string[] IN_VEHICLE_DIST_TS = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38" };
    private static readonly string[] IN_VEHICLE_DIST_UK = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };
    private static readonly string[] IN_VEHICLE_DIST_UP = new[] { "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96" };
    private static readonly string[] IN_VEHICLE_DIST_WB = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98" };

    private static readonly string[] IN_NON_STANDARD_STATE_OR_UT = new[] { "DD" };
    private static readonly string[] IN_OLD_STATES = new[] { "UL", "OR", "UA" };
    private static readonly string[] IN_OLD_UNION_TERRITORIES = new[] { "CT", "DN" };
    private static readonly string[] IN_STATES = new[] { "AP", "AR", "AS", "BR", "CG", "GA", "GJ", "HR", "HP", "JH", "KA", "KL", "MP", "MH", "MN", "ML", "MZ", "NL", "OD", "PB", "RJ", "SK", "TN", "TS", "TR", "UP", "UK", "WB", "UT" };
    private static readonly string[] IN_UNION_TERRITORIES = new[] { "AN", "CH", "DH", "DL", "JK", "LA", "LD", "PY" };
    private static readonly string[] IN_VEHICLE_DIPLOMATIC_CODES = new[] { "CC", "CD", "UN" };
    private static readonly string[] IN_VEHICLE_FOREIGN_MISSION_CODES_STR = new[] { "84", "85", "89", "93", "94", "95", "97", "98", "99", "102", "104", "105", "106", "109", "111", "112", "113", "117", "119", "120", "121", "122", "123", "125", "126", "128", "133", "134", "135", "137", "141", "145", "147", "149", "152", "153", "155", "156", "157", "159", "160" };

    private static readonly IReadOnlyDictionary<string, string[]> STATE_RTO_DISTRICT_MAP = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["AN"] = IN_VEHICLE_DIST_AN,
        ["AP"] = IN_VEHICLE_DIST_AP,
        ["AR"] = IN_VEHICLE_DIST_AR,
        ["AS"] = IN_VEHICLE_DIST_AS,
        ["BR"] = IN_VEHICLE_DIST_BR,
        ["CG"] = IN_VEHICLE_DIST_CG,
        ["CH"] = IN_VEHICLE_DIST_CH,
        ["DD"] = IN_VEHICLE_DIST_DD,
        ["DL"] = IN_VEHICLE_DIST_DL,
        ["DN"] = IN_VEHICLE_DIST_DN,
        ["GA"] = IN_VEHICLE_DIST_GA,
        ["GJ"] = IN_VEHICLE_DIST_GJ,
        ["HP"] = IN_VEHICLE_DIST_HP,
        ["HR"] = IN_VEHICLE_DIST_HR,
        ["JH"] = IN_VEHICLE_DIST_JH,
        ["JK"] = IN_VEHICLE_DIST_JK,
        ["KA"] = IN_VEHICLE_DIST_KA,
        ["KL"] = IN_VEHICLE_DIST_KL,
        ["LA"] = IN_VEHICLE_DIST_LA,
        ["LD"] = IN_VEHICLE_DIST_LD,
        ["MH"] = IN_VEHICLE_DIST_MH,
        ["ML"] = IN_VEHICLE_DIST_ML,
        ["MN"] = IN_VEHICLE_DIST_MN,
        ["MP"] = IN_VEHICLE_DIST_MP,
        ["MZ"] = IN_VEHICLE_DIST_MZ,
        ["NL"] = IN_VEHICLE_DIST_NL,
        ["OD"] = IN_VEHICLE_DIST_OD,
        ["OR"] = IN_VEHICLE_DIST_OR,
        ["PB"] = IN_VEHICLE_DIST_PB,
        ["PY"] = IN_VEHICLE_DIST_PY,
        ["RJ"] = IN_VEHICLE_DIST_RJ,
        ["SK"] = IN_VEHICLE_DIST_SK,
        ["TN"] = IN_VEHICLE_DIST_TN,
        ["TR"] = IN_VEHICLE_DIST_TR,
        ["TS"] = IN_VEHICLE_DIST_TS,
        ["UK"] = IN_VEHICLE_DIST_UK,
        ["UP"] = IN_VEHICLE_DIST_UP,
        ["WB"] = IN_VEHICLE_DIST_WB,
    };

    private static readonly HashSet<int> IN_VEHICLE_FOREIGN_MISSION_CODES_SET = IN_VEHICLE_FOREIGN_MISSION_CODES_STR
        .Select(int.Parse)
        .ToHashSet();

    private static readonly IReadOnlyDictionary<string, HashSet<string>> STATE_RTO_DISTRICT_SETS = STATE_RTO_DISTRICT_MAP.ToDictionary(
        pair => pair.Key,
        pair => pair.Value.ToHashSet(StringComparer.OrdinalIgnoreCase),
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> TWO_FACTOR_REGISTRATION_PREFIX = BuildRegistrationPrefixes();

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs = (replacementPairs?.ToArray() ?? new (string, string)[]
    {
        ("-", string.Empty),
        (" ", string.Empty),
        (":", string.Empty),
    }).ToImmutableArray();

    private static HashSet<string> BuildRegistrationPrefixes()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        set.UnionWith(IN_UNION_TERRITORIES);
        set.UnionWith(IN_STATES);
        set.UnionWith(IN_OLD_STATES);
        set.UnionWith(IN_OLD_UNION_TERRITORIES);
        set.UnionWith(IN_NON_STANDARD_STATE_OR_UT);
        return set;
    }

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        return CheckVehicleRegistration(sanitized);
    }

    private static bool CheckVehicleRegistration(string value)
    {
        var sanitized = value.ToUpperInvariant();
        if (sanitized.Length < 8)
        {
            return false;
        }

        var prefix = sanitized[..2];
        if (!TWO_FACTOR_REGISTRATION_PREFIX.Contains(prefix))
        {
            return false;
        }

        var districtCode = string.Empty;
        if (sanitized.Length > 2 && char.IsDigit(sanitized[2]))
        {
            if (sanitized.Length > 3 && char.IsDigit(sanitized[3]))
            {
                districtCode = sanitized.Substring(2, 2);
            }
            else
            {
                districtCode = sanitized[2].ToString();
            }
        }

        if (sanitized.Length < 4)
        {
            return false;
        }

        var registrationDigits = sanitized[^4..];
        if (!registrationDigits.All(char.IsDigit))
        {
            return false;
        }

        if (!int.TryParse(registrationDigits, out var registrationNumber) || registrationNumber <= 0 || registrationNumber > 9999)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(districtCode) && STATE_RTO_DISTRICT_SETS.TryGetValue(prefix, out var districts) && districts.Contains(districtCode))
        {
            return true;
        }

        foreach (var diplomaticCode in IN_VEHICLE_DIPLOMATIC_CODES)
        {
            var index = sanitized.IndexOf(diplomaticCode, StringComparison.Ordinal);
            if (index >= 0)
            {
                var vehiclePrefix = sanitized[..index];
                if (vehiclePrefix.Length > 0 && vehiclePrefix.All(char.IsDigit) && int.TryParse(vehiclePrefix, out var numericPrefix))
                {
                    if ((numericPrefix >= 1 && numericPrefix <= 80) || IN_VEHICLE_FOREIGN_MISSION_CODES_SET.Contains(numericPrefix))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
