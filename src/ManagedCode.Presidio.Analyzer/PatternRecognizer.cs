using System.Text.RegularExpressions;
using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Regular-expression driven recognizer aligned with the Python pattern recognizer.
/// </summary>
public class PatternRecognizer : LocalRecognizer
{
    private readonly IReadOnlyList<Pattern> _patterns;
    private RegexOptions _globalRegexOptions;
    private readonly IReadOnlyList<string> _context;
    private readonly List<CompiledPattern> _compiledPatterns;

    public PatternRecognizer(
        string supportedEntity,
        IEnumerable<Pattern>? patterns = null,
        IEnumerable<string>? denyList = null,
        IEnumerable<string>? context = null,
        string? name = null,
        string supportedLanguage = "en",
        double denyListScore = 1.0,
        RegexOptions? globalRegexOptions = null,
        string version = "0.0.1")
        : base(new[] { supportedEntity }, name, supportedLanguage, version, context)
    {
        if (string.IsNullOrWhiteSpace(supportedEntity))
        {
            throw new ArgumentException("Pattern recognizer requires a supported entity.", nameof(supportedEntity));
        }

        var patternList = patterns?.ToList() ?? new List<Pattern>();
        if (denyList is not null)
        {
            var denyListPattern = BuildDenyListPattern(denyList, denyListScore);
            patternList.Add(denyListPattern);
        }

        if (patternList.Count == 0)
        {
            throw new ArgumentException("Pattern recognizer requires either patterns or a deny-list.");
        }

        _patterns = patternList;
        _context = context?.ToArray() ?? Array.Empty<string>();
        _globalRegexOptions = globalRegexOptions ?? (RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
        _compiledPatterns = _patterns.Select(pattern => new CompiledPattern(pattern)).ToList();
    }

    protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts)
    {
        var results = new List<RecognizerResult>();
        foreach (var compiled in _compiledPatterns)
        {
            var regex = compiled.GetRegex(_globalRegexOptions);
            var matches = regex.Matches(text ?? string.Empty);
            foreach (Match match in matches)
            {
                if (!match.Success || match.Length == 0)
                {
                    continue;
                }

                var candidate = match.Value;
                var score = compiled.Pattern.Score;
                var validationResult = ValidateResult(candidate);
                if (validationResult is not null)
                {
                    score = validationResult.Value ? MaxScore : MinScore;
                }

                var invalidationResult = InvalidateResult(candidate);
                if (invalidationResult is true)
                {
                    score = MinScore;
                }

                if (score <= MinScore)
                {
                    continue;
                }

                var explanation = BuildRegexExplanation(
                    Name,
                    compiled.Pattern.Name,
                    compiled.Pattern.Regex,
                    compiled.Pattern.Score,
                    validationResult ?? false,
                    _globalRegexOptions);

                var result = new RecognizerResult(
                    SupportedEntities.First(),
                    new TextSpan(match.Index, match.Index + match.Length),
                    score,
                    explanation,
                    new Dictionary<string, object?>
                    {
                        [RecognitionMetadataKeys.RecognizerName] = Name,
                        [RecognitionMetadataKeys.RecognizerIdentifier] = Id,
                    });

                results.Add(result);
            }
        }

        return EntityRecognizer.RemoveDuplicates(results);
    }

    protected virtual bool? ValidateResult(string patternText) => null;

    protected virtual bool? InvalidateResult(string patternText) => null;

    internal void SetGlobalRegexOptions(RegexOptions options)
    {
        _globalRegexOptions = options;
    }

    protected static AnalysisExplanation BuildRegexExplanation(
        string recognizerName,
        string patternName,
        string pattern,
        double originalScore,
        bool validationResult,
        RegexOptions options)
    {
        var explanation = new AnalysisExplanation(
            recognizerName,
            originalScore,
            patternName,
            pattern,
            validationResult
                ? originalScore
                : null,
            textualExplanation: $"Detected by `{recognizerName}` using pattern `{patternName}`",
            regexFlags: (int)options);

        explanation.SetImprovedScore(validationResult ? MaxScore : originalScore);
        return explanation;
    }

    private static Pattern BuildDenyListPattern(IEnumerable<string> denyList, double score)
    {
        var escaped = denyList
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(Regex.Escape)
            .ToArray();

        if (escaped.Length == 0)
        {
            throw new ArgumentException("Deny list must contain at least one non-empty value.", nameof(denyList));
        }

        var regex = $"(?:^|(?<=\\W))({string.Join("|", escaped)})(?:(?=\\W)|$)";
        return new Pattern("deny_list", regex, score);
    }

    private sealed class CompiledPattern(Pattern pattern)
    {
        private Regex? _compiledRegex;
        private RegexOptions _options;

        public Pattern Pattern { get; } = pattern;

        public Regex GetRegex(RegexOptions options)
        {
            if (_compiledRegex is null || _options != options)
            {
                _compiledRegex = new Regex(Pattern.Regex, options);
                _options = options;
            }

            return _compiledRegex;
        }
    }
}
