using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Base abstraction for enhancing recognizer scores using surrounding context.
/// </summary>
public abstract class ContextAwareEnhancer(
    double contextSimilarityFactor,
    double minScoreWithContextSimilarity,
    int contextPrefixCount,
    int contextSuffixCount)
{
    public const double MinScore = 0;
    public const double MaxScore = 1.0;

    public double ContextSimilarityFactor { get; } = contextSimilarityFactor;

    public double MinScoreWithContextSimilarity { get; } = minScoreWithContextSimilarity;

    public int ContextPrefixCount { get; } = contextPrefixCount;

    public int ContextSuffixCount { get; } = contextSuffixCount;

    public abstract IReadOnlyCollection<RecognizerResult> EnhanceUsingContext(
        string text,
        IReadOnlyCollection<RecognizerResult> rawResults,
        NlpArtifacts nlpArtifacts,
        IReadOnlyCollection<EntityRecognizer> recognizers,
        IReadOnlyCollection<string>? context = null);
}
