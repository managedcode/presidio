using System.Diagnostics.CodeAnalysis;
using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Lemma-based context enhancer mirroring the Python implementation.
/// </summary>
[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Matches Presidio Python implementation which uses lowercase normalization.")]
public sealed class LemmaContextAwareEnhancer(
    double contextSimilarityFactor = 0.35,
    double minScoreWithContextSimilarity = 0.4,
    int contextPrefixCount = 5,
    int contextSuffixCount = 0) : ContextAwareEnhancer(
        contextSimilarityFactor,
        minScoreWithContextSimilarity,
        contextPrefixCount,
        contextSuffixCount)
{
    public override IReadOnlyCollection<RecognizerResult> EnhanceUsingContext(
        string text,
        IReadOnlyCollection<RecognizerResult> rawResults,
        NlpArtifacts nlpArtifacts,
        IReadOnlyCollection<EntityRecognizer> recognizers,
        IReadOnlyCollection<string>? context = null)
    {
        ArgumentNullException.ThrowIfNull(rawResults);
        ArgumentNullException.ThrowIfNull(recognizers);

        if (rawResults.Count == 0)
        {
            return Array.Empty<RecognizerResult>();
        }

        var results = rawResults.Select(CloneResult).ToList();

        if (nlpArtifacts is null || !nlpArtifacts.HasTokens)
        {
            return results;
        }

        var recognizerMap = recognizers.ToDictionary(recognizer => recognizer.Id, StringComparer.Ordinal);
        var normalizedContext = context?.Select(word => word.ToLowerInvariant()).ToList() ?? new List<string>();

        foreach (var result in results)
        {
            var recognizer = TryResolveRecognizer(result, recognizerMap);
            if (recognizer is null || recognizer.Context.Count == 0)
            {
                continue;
            }

            if (IsResultAlreadyBoosted(result))
            {
                continue;
            }

            var word = GetSpanText(text, result);
            var surroundingWords = ExtractSurroundingWords(nlpArtifacts, word, result.Start);
            surroundingWords.AddRange(normalizedContext);

            var supportiveContext = FindSupportiveWordInContext(
                surroundingWords,
                recognizer.Context);

            if (string.IsNullOrEmpty(supportiveContext))
            {
                continue;
            }

            var updatedScore = Math.Min(
                ContextAwareEnhancer.MaxScore,
                Math.Max(MinScoreWithContextSimilarity, result.Score + ContextSimilarityFactor));

            result.UpdateScore(updatedScore);
            result.AnalysisExplanation?.SetSupportiveContextWord(supportiveContext);
            result.SetMetadata(RecognitionMetadataKeys.IsScoreEnhancedByContext, true);
        }

        return results;
    }

    private static RecognizerResult CloneResult(RecognizerResult result)
    {
        var metadataCopy = new Dictionary<string, object?>(result.Metadata, StringComparer.Ordinal);
        return new RecognizerResult(
            result.EntityType,
            result.Span,
            result.Score,
            result.AnalysisExplanation,
            metadataCopy);
    }

    private static EntityRecognizer? TryResolveRecognizer(
        RecognizerResult result,
        IReadOnlyDictionary<string, EntityRecognizer> recognizers)
    {
        if (!result.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerIdentifier, out var recognizerId) || string.IsNullOrEmpty(recognizerId))
        {
            return null;
        }

        return recognizers.TryGetValue(recognizerId, out var recognizer) ? recognizer : null;
    }

    private static bool IsResultAlreadyBoosted(RecognizerResult result) =>
        result.TryGetMetadata<bool>(RecognitionMetadataKeys.IsScoreEnhancedByContext, out var boosted) && boosted;

    private List<string> ExtractSurroundingWords(NlpArtifacts artifacts, string word, int start)
    {
        if (!artifacts.HasTokens)
        {
            return new List<string> { string.Empty };
        }

        var tokens = artifacts.Tokens;
        var tokenIndex = FindTokenIndex(tokens, word, start);

        var backwards = AddNWords(tokens, tokenIndex, ContextPrefixCount, artifacts.Keywords, backwards: true);
        var forwards = AddNWords(tokens, tokenIndex, ContextSuffixCount, artifacts.Keywords, backwards: false);

        return backwards
            .Concat(forwards)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static int FindTokenIndex(IReadOnlyList<Token> tokens, string word, int start)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var coversIndex = start >= token.Start && start < token.End;
            if (token.Start == start || coversIndex)
            {
                return i;
            }
        }

        throw new InvalidOperationException($"Token for '{word}' starting at {start} was not found in NLP artifacts.");
    }

    private static IEnumerable<string> AddNWords(
        IReadOnlyList<Token> tokens,
        int index,
        int wordCount,
        IReadOnlyCollection<string> keywords,
        bool backwards)
    {
        if (tokens.Count == 0 || index < 0 || index >= tokens.Count)
        {
            return Array.Empty<string>();
        }

        var collected = new List<string>();
        var remaining = Math.Max(0, wordCount) + 1;
        var current = index;

        while (current >= 0 && current < tokens.Count && remaining > 0)
        {
            var lemma = tokens[current].Lemma.ToLowerInvariant();
            if (keywords.Contains(lemma))
            {
                collected.Add(lemma);
                remaining -= 1;
            }

            current = backwards ? current - 1 : current + 1;
        }

        return collected;
    }

    private static string FindSupportiveWordInContext(
        IReadOnlyCollection<string> contextList,
        IReadOnlyCollection<string> recognizerContextList)
    {
        if (contextList.Count == 0 || recognizerContextList.Count == 0)
        {
            return string.Empty;
        }

        foreach (var rawWord in recognizerContextList)
        {
            if (string.IsNullOrEmpty(rawWord))
            {
                continue;
            }

            var normalized = rawWord.ToLowerInvariant();
            var hasMatch = contextList.Any(keyword =>
                keyword.Contains(normalized, StringComparison.Ordinal));

            if (hasMatch)
            {
                return normalized;
            }
        }

        return string.Empty;
    }

    private static string GetSpanText(string text, RecognizerResult result)
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
