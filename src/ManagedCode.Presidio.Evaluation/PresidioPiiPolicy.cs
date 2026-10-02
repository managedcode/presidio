namespace ManagedCode.Presidio.Evaluation;

using ManagedCode.Presidio.Analyzer;

/// <summary>
/// Explicit entity selection and score threshold used when inspecting privacy evidence.
/// </summary>
public sealed class PresidioPiiPolicy
{
    public PresidioPiiPolicy(
        string language,
        IReadOnlyCollection<string> entityTypes,
        double scoreThreshold)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentNullException.ThrowIfNull(entityTypes);

        if (entityTypes.Count == 0 || entityTypes.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty entity type must be selected.", nameof(entityTypes));
        }

        if (scoreThreshold is < 0 or > 1 || double.IsNaN(scoreThreshold))
        {
            throw new ArgumentOutOfRangeException(nameof(scoreThreshold), "The score threshold must be between zero and one.");
        }

        Language = language;
        EntityTypes = entityTypes.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        ScoreThreshold = scoreThreshold;
    }

    public string Language { get; }

    public IReadOnlyCollection<string> EntityTypes { get; }

    public double ScoreThreshold { get; }

    internal void ValidateCoverage(AnalyzerEngine analyzer)
    {
        ArgumentNullException.ThrowIfNull(analyzer);

        var supportedEntities = analyzer.GetSupportedEntities(Language).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unsupportedEntities = EntityTypes
            .Where(entity => !supportedEntities.Contains(entity))
            .ToArray();

        if (unsupportedEntities.Length > 0)
        {
            throw new ArgumentException(
                $"The analyzer has no recognizer for selected entity type(s) '{string.Join("', '", unsupportedEntities)}' in language '{Language}'.");
        }
    }
}
