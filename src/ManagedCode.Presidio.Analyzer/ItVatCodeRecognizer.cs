namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Italian VAT codes (Partita IVA) using checksum validation.
/// </summary>
public sealed class ItVatCodeRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "it",
    string supportedEntity = "IT_VAT_CODE",
    IEnumerable<(string Search, string Replacement)>? replacementPairs = null) : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private static readonly Pattern[] DefaultPatterns =
    {
        new("IT Vat code (piva)", @"\b([0-9][ _]?){11}\b", 0.1),
    };

    private static readonly string[] DefaultContext =
    {
        "piva",
        "partita iva",
        "pi",
    };

    private readonly IReadOnlyList<(string Search, string Replacement)> _replacementPairs =
        replacementPairs?.ToArray() ?? new (string Search, string Replacement)[]
        {
            ("-", string.Empty),
            (" ", string.Empty),
            ("_", string.Empty),
        };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        var sanitized = EntityRecognizer.SanitizeValue(patternText, _replacementPairs);
        if (sanitized.Length != 11 || sanitized.Any(ch => !char.IsDigit(ch)))
        {
            return false;
        }

        if (sanitized == "00000000000")
        {
            return false;
        }

        var x = 0;
        var y = 0;
        for (var i = 0; i < 5; i++)
        {
            x += sanitized[2 * i] - '0';

            var tmp = (sanitized[2 * i + 1] - '0') * 2;
            if (tmp > 9)
            {
                tmp -= 9;
            }

            y += tmp;
        }

        var t = (x + y) % 10;
        var c = (10 - t) % 10;
        return c == sanitized[10] - '0';
    }
}
using System.Linq;
