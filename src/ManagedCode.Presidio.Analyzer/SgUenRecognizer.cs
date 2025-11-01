namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Singapore UEN identifiers with checksum validation.
/// </summary>
public sealed class SgUenRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "SG_UEN") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("UEN (low)", "\\b\\d{8}[A-Z]\\b|\\b\\d{9}[A-Z]\\b|\\b(T|S)\\d{2}[A-Z]{2}\\d{4}[A-Z]\\b", 0.3),
    };

    private static readonly string[] DefaultContext =
    {
        "uen",
        "unique entity number",
        "business registration",
        "acra",
    };

    private static readonly int[] FormatAWeights = { 10, 4, 9, 3, 8, 2, 7, 1 };
    private const string FormatAAlphabet = "XMKECAWLJDB";
    private static readonly int[] FormatBWeights = { 10, 8, 6, 4, 9, 7, 5, 3, 1 };
    private const string FormatBAlphabet = "ZKCMDNERGWH";
    private static readonly int[] FormatCWeights = { 4, 3, 5, 3, 10, 2, 2, 5, 7 };
    private const string FormatCAlphabet = "ABCDEFGHJKLMNPQRSTUVWX0123456789";
    private static readonly HashSet<string> FormatCPrefixes = new(StringComparer.OrdinalIgnoreCase) { "T", "S", "R" };
    private static readonly HashSet<string> FormatCEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "LP", "LL", "FC", "PF", "RF", "MQ", "MM", "NB", "CC", "CS", "MB", "FM", "GS", "DP", "CP", "NR",
        "CM", "CD", "MD", "HS", "VH", "CH", "MH", "CL", "XL", "CX", "HC", "RP", "TU", "TC", "FB", "FN",
        "PA", "PB", "SS", "MC", "SM", "GA", "GB",
    };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = patternText.Trim().ToUpperInvariant();
        return sanitized.Length switch
        {
            9 => ValidateFormatA(sanitized),
            10 when char.IsLetter(sanitized[0]) => ValidateFormatC(sanitized),
            10 => ValidateFormatB(sanitized),
            _ => false,
        };
    }

    private static bool ValidateFormatA(string uen)
    {
        var checksum = FormatAAlphabet[WeightedSum(uen[..^1], FormatAWeights) % 11];
        return uen[^1] == checksum;
    }

    private static bool ValidateFormatB(string uen)
    {
        if (!int.TryParse(uen[..4], out var year) || year > DateTime.UtcNow.Year)
        {
            return false;
        }

        var checksum = FormatBAlphabet[WeightedSum(uen[..^1], FormatBWeights) % 11];
        return uen[^1] == checksum;
    }

    private static bool ValidateFormatC(string uen)
    {
        if (!FormatCPrefixes.Contains(uen[0].ToString()))
        {
            return false;
        }

        var entityType = uen.Substring(3, 2);
        if (!FormatCEntityTypes.Contains(entityType))
        {
            return false;
        }

        var weightedSum = 0;
        for (var i = 0; i < uen.Length - 1; i++)
        {
            var index = FormatCAlphabet.IndexOf(uen[i]);
            if (index < 0)
            {
                return false;
            }

            weightedSum += index * FormatCWeights[i];
        }

        var checksumIndex = (weightedSum - 5) % 11;
        if (checksumIndex < 0)
        {
            checksumIndex += 11;
        }

        var checksumChar = FormatCAlphabet[checksumIndex];
        return uen[^1] == checksumChar;
    }

    private static int WeightedSum(string digits, IReadOnlyList<int> weights) =>
        digits.Zip(weights, (ch, weight) => (ch - '0') * weight).Sum();
}
