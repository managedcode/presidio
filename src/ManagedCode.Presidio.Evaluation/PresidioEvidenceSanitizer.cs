using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;

namespace ManagedCode.Presidio.Evaluation;

/// <summary>Controls how explicitly selected PII findings are removed from evaluation evidence.</summary>
public enum PresidioEvidenceRedactionMode
{
    Redact,
    ReplaceWithEntity,
}

/// <summary>A PII finding with source offsets that refer to the unsanitized evidence.</summary>
public sealed record PresidioEvidenceFinding(string EntityType, int Start, int End, double Score);

/// <summary>Sanitized text and findings detected in the original evidence.</summary>
public sealed record PresidioEvidenceSanitizationResult(
    string Text,
    IReadOnlyList<PresidioEvidenceFinding> Findings);

/// <summary>
/// Runs Presidio's real analyzer and anonymizer over evidence explicitly selected by the caller.
/// This type never rewrites chat messages or tool arguments implicitly.
/// </summary>
public sealed class PresidioEvidenceSanitizer(AnalyzerEngine analyzer, AnonymizerEngine anonymizer)
{
    private readonly AnalyzerEngine _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
    private readonly AnonymizerEngine _anonymizer = anonymizer ?? throw new ArgumentNullException(nameof(anonymizer));

    public PresidioEvidenceSanitizationResult Sanitize(
        string evidence,
        PresidioPiiPolicy policy,
        PresidioEvidenceRedactionMode redactionMode)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(policy);
        policy.ValidateCoverage(_analyzer);

        var analysis = _analyzer.Analyze(
            evidence,
            policy.Language,
            entities: policy.EntityTypes,
            scoreThreshold: policy.ScoreThreshold);

        var findings = analysis
            .Select(result => new PresidioEvidenceFinding(result.EntityType, result.Start, result.End, result.Score))
            .ToArray();

        var operatorName = redactionMode switch
        {
            PresidioEvidenceRedactionMode.Redact => "redact",
            PresidioEvidenceRedactionMode.ReplaceWithEntity => "replace",
            _ => throw new ArgumentOutOfRangeException(nameof(redactionMode)),
        };

        var anonymized = _anonymizer.Anonymize(
            evidence,
            analysis,
            new Dictionary<string, OperatorConfig>(StringComparer.Ordinal)
            {
                ["DEFAULT"] = new OperatorConfig(operatorName),
            });

        return new PresidioEvidenceSanitizationResult(anonymized.Text ?? string.Empty, findings);
    }
}
