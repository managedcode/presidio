using System.Text.RegularExpressions;
using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Entry point for Presidio Analyzer. Ported from the Python implementation.
/// </summary>
public sealed class AnalyzerEngine : IDisposable
{
    private readonly RecognizerRegistry _registry;
    private readonly INlpEngine _nlpEngine;
    private readonly AppTracer _appTracer;
    private readonly bool _logDecisionProcess;
    private readonly double _defaultScoreThreshold;
    private readonly IReadOnlyCollection<string> _supportedLanguages;
    private readonly ContextAwareEnhancer _contextAwareEnhancer;

    public AnalyzerEngine(
        RecognizerRegistry? registry = null,
        INlpEngine? nlpEngine = null,
        AppTracer? appTracer = null,
        bool logDecisionProcess = false,
        double defaultScoreThreshold = 0,
        IReadOnlyCollection<string>? supportedLanguages = null,
        ContextAwareEnhancer? contextAwareEnhancer = null)
    {
        _supportedLanguages = supportedLanguages is { Count: > 0 }
            ? supportedLanguages
            : new[] { "en" };

        _nlpEngine = nlpEngine ?? new NlpEngineProvider().CreateEngine();
        EnsureNlpEngineLoaded();

        _appTracer = appTracer ?? new AppTracer();
        _logDecisionProcess = logDecisionProcess;
        _defaultScoreThreshold = defaultScoreThreshold;

        if (registry is null)
        {
            var provider = new RecognizerRegistryProvider();
            _registry = provider.CreateRecognizerRegistry(_nlpEngine, _supportedLanguages);
        }
        else
        {
            if (!HasMatchingLanguages(registry.SupportedLanguages, _supportedLanguages))
            {
                throw new ArgumentException(
                    $"Misconfigured engine. Registry languages '{string.Join(",", registry.SupportedLanguages)}' do not match analyzer languages '{string.Join(",", _supportedLanguages)}'.");
            }

            _registry = registry;
        }

        _contextAwareEnhancer = contextAwareEnhancer ?? new LemmaContextAwareEnhancer();
    }

    public IReadOnlyCollection<EntityRecognizer> GetRecognizers(string? language = null)
    {
        var languages = language is null ? _supportedLanguages : new[] { language };
        var recognizers = new List<EntityRecognizer>();
        foreach (var lang in languages)
        {
            recognizers.AddRange(_registry.GetRecognizers(lang, allFields: true));
        }

        return recognizers.Distinct().ToArray();
    }

    public IReadOnlyCollection<string> GetSupportedEntities(string? language = null)
    {
        var languages = language is null ? _supportedLanguages : new[] { language };
        return _registry.GetSupportedEntities(languages);
    }

    public IReadOnlyCollection<RecognizerResult> Analyze(
        string text,
        string language,
        IReadOnlyCollection<string>? entities = null,
        string? correlationId = null,
        double? scoreThreshold = null,
        bool returnDecisionProcess = false,
        IReadOnlyCollection<EntityRecognizer>? adHocRecognizers = null,
        IReadOnlyCollection<string>? context = null,
        IReadOnlyCollection<string>? allowList = null,
        AllowListMatch allowListMatch = AllowListMatch.Exact,
        RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase,
        NlpArtifacts? nlpArtifacts = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        EnsureNlpEngineLoaded();

        var allFields = entities is null || entities.Count == 0;
        var recognizers = _registry.GetRecognizers(
            language,
            entities,
            allFields,
            adHocRecognizers);

        if (allFields)
        {
            entities = GetSupportedEntities(language);
        }

        try
        {
            nlpArtifacts ??= _nlpEngine.ProcessText(text, language);
        }
        catch (NotSupportedException ex) when (ex.Message.Contains("Language", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("No matching recognizers were found to serve the request.", ex);
        }
        if (_logDecisionProcess)
        {
            _appTracer.Trace(correlationId, $"nlp_artifacts: {nlpArtifacts.ToJson()}");
        }

        var results = new List<RecognizerResult>();
        foreach (var recognizer in recognizers)
        {
            if (!recognizer.IsLoaded)
            {
                recognizer.Load();
            }

            var currentResults = recognizer.Analyze(text, entities!, nlpArtifacts);
            if (currentResults is null || currentResults.Count == 0)
            {
                continue;
            }

            AddRecognizerMetadata(currentResults, recognizer);
            results.AddRange(currentResults);
        }

        var enhanced = EnhanceUsingContext(text, results, nlpArtifacts, recognizers, context);

        if (_logDecisionProcess)
        {
            var serialized = enhanced.Select(result => result.ToDictionary()).ToArray();
            _appTracer.Trace(correlationId, $"results: {System.Text.Json.JsonSerializer.Serialize(serialized)}");
        }

        var deduplicated = RecognizerResult.RemoveDuplicates(enhanced);
        var filtered = RemoveLowScores(deduplicated, scoreThreshold);

        if (allowList is not null && allowList.Count > 0)
        {
            filtered = RemoveAllowList(filtered, allowList, text, regexOptions, allowListMatch);
        }

        if (!returnDecisionProcess)
        {
            foreach (var result in filtered)
            {
                result.ClearAnalysisExplanation();
            }
        }

        return filtered.ToArray();
    }

    public void Dispose()
    {
        _nlpEngine.Dispose();
    }

    private static void AddRecognizerMetadata(IEnumerable<RecognizerResult> results, EntityRecognizer recognizer)
    {
        foreach (var result in results)
        {
            if (!result.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerIdentifier, out _))
            {
                result.SetMetadata(RecognitionMetadataKeys.RecognizerIdentifier, recognizer.Id);
            }

            if (!result.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerName, out _))
            {
                result.SetMetadata(RecognitionMetadataKeys.RecognizerName, recognizer.Name);
            }
        }
    }

    private IReadOnlyCollection<RecognizerResult> EnhanceUsingContext(
        string text,
        IReadOnlyCollection<RecognizerResult> rawResults,
        NlpArtifacts artifacts,
        IReadOnlyCollection<EntityRecognizer> recognizers,
        IReadOnlyCollection<string>? context)
    {
        if (rawResults.Count == 0)
        {
            return rawResults;
        }

        var enhancedResults = new List<RecognizerResult>();
        foreach (var recognizer in recognizers)
        {
            var recognizerResults = rawResults
                .Where(result => result.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerIdentifier, out var id) &&
                                 string.Equals(id, recognizer.Id, StringComparison.Ordinal))
                .ToArray();

            if (recognizerResults.Length == 0)
            {
                continue;
            }

            var otherResults = rawResults.Except(recognizerResults).ToArray();
            var updated = recognizer.EnhanceUsingContext(text, recognizerResults, otherResults, artifacts, context);
            enhancedResults.AddRange(updated);
        }

        return _contextAwareEnhancer.EnhanceUsingContext(text, enhancedResults, artifacts, recognizers, context);
    }

    private IReadOnlyCollection<RecognizerResult> RemoveLowScores(
        IEnumerable<RecognizerResult> results,
        double? scoreThreshold)
    {
        var threshold = scoreThreshold ?? _defaultScoreThreshold;
        return results
            .Where(result => result.Score >= threshold)
            .ToArray();
    }

    private static IReadOnlyCollection<RecognizerResult> RemoveAllowList(
        IEnumerable<RecognizerResult> results,
        IReadOnlyCollection<string> allowList,
        string text,
        RegexOptions regexOptions,
        AllowListMatch allowListMatch)
    {
        if (allowListMatch == AllowListMatch.Regex)
        {
            var pattern = string.Join("|", allowList);
            var regex = new Regex(pattern, regexOptions);
            return results
                .Where(result => !regex.IsMatch(GetSegment(text, result)))
                .ToArray();
        }

        return results
            .Where(result => !allowList.Contains(GetSegment(text, result)))
            .ToArray();
    }

    private void EnsureNlpEngineLoaded()
    {
        if (!_nlpEngine.IsLoaded)
        {
            _nlpEngine.Load();
        }
    }

    private static bool HasMatchingLanguages(
        IReadOnlyCollection<string> registryLanguages,
        IReadOnlyCollection<string> engineLanguages)
    {
        return registryLanguages.Count == engineLanguages.Count
               && !registryLanguages.Except(engineLanguages, StringComparer.OrdinalIgnoreCase).Any();
    }

    private static string GetSegment(string text, RecognizerResult result)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var safeStart = Math.Clamp(result.Start, 0, text.Length);
        var safeEnd = Math.Clamp(result.End, 0, text.Length);
        if (safeEnd <= safeStart)
        {
            return string.Empty;
        }

        return text[safeStart..safeEnd];
    }
}

public enum AllowListMatch
{
    Exact,
    Regex,
}
