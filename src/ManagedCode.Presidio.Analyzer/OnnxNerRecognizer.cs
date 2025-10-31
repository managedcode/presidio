using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Entity recognizer backed by ONNX token classification results.
/// </summary>
public sealed class OnnxNerRecognizer(string language, IReadOnlyCollection<string> supportedEntities) : EntityRecognizer(supportedEntities, name: "OnnxNerRecognizer", supportedLanguage: language, context: Array.Empty<string>())
{
    protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts)
    {
        if (artifacts.Entities.Count == 0)
        {
            return Array.Empty<RecognizerResult>();
        }

        var filter = entities.Count == 0
            ? SupportedEntities
            : entities.Intersect(SupportedEntities, StringComparer.OrdinalIgnoreCase).ToArray();

        var results = new List<RecognizerResult>();
        foreach (var entity in artifacts.Entities)
        {
            if (!filter.Contains(entity.Label, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var span = new TextSpan(entity.Start, entity.End);
            var explanation = new AnalysisExplanation(Name, entity.Score, patternName: entity.RawLabel ?? "ONNX", pattern: Name);
            var metadata = new Dictionary<string, object?>
            {
                [RecognitionMetadataKeys.RecognizerIdentifier] = Id,
                [RecognitionMetadataKeys.RecognizerName] = Name,
                ["raw_label"] = entity.RawLabel,
                ["source"] = entity.Source,
            };

            if (entity.Distribution is { } embedding)
            {
                metadata["label_distribution"] = embedding.Vector.ToArray();
            }

            results.Add(new RecognizerResult(entity.Label, span, entity.Score, explanation, metadata));
        }

        return results;
    }
}
