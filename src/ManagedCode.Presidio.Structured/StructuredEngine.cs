using System.Text.Json.Nodes;
using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;
using ManagedCode.Presidio.Core;
using CoreRecognizerResult = ManagedCode.Presidio.Core.RecognizerResult;

namespace ManagedCode.Presidio.Structured;

/// <summary>Analyzes and anonymizes JSON object properties with Presidio's text pipelines.</summary>
public sealed class StructuredEngine(AnalyzerEngine analyzer, AnonymizerEngine anonymizer)
{
    private readonly AnalyzerEngine _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
    private readonly AnonymizerEngine _anonymizer = anonymizer ?? throw new ArgumentNullException(nameof(anonymizer));

    public StructuredAnalysis Analyze(JsonNode data, string language, double scoreThreshold = 0)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ValidateThreshold(scoreThreshold);
        ValidateRoot(data);

        var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in EnumerateStringFields(data))
        {
            var finding = _analyzer.Analyze(field.Value, language, scoreThreshold: scoreThreshold).FirstOrDefault();
            if (finding is not null)
            {
                mapping[field.Path] = finding.EntityType;
            }
        }

        return new StructuredAnalysis(mapping);
    }

    public JsonNode Anonymize(
        JsonNode data,
        StructuredAnalysis analysis,
        IReadOnlyDictionary<string, OperatorConfig> operators)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(operators);
        ValidateRoot(data);

        var result = data.DeepClone();
        var values = EnumerateStringFields(result).ToDictionary(field => field.Path, StringComparer.Ordinal);
        foreach (var (path, entityType) in analysis.EntityMapping)
        {
            if (!values.TryGetValue(path, out var field))
            {
                throw new InvalidOperationException($"Structured analysis path '{path}' does not exist in the supplied JSON document.");
            }

            var config = operators.TryGetValue(entityType, out var specific)
                ? specific
                : operators.TryGetValue("DEFAULT", out var fallback)
                    ? fallback
                    : throw new InvalidOperationException($"No anonymizer operator is configured for '{entityType}' or DEFAULT.");

            var recognizerResult = new CoreRecognizerResult(entityType, new TextSpan(0, field.Value.Length), 1);
            var anonymized = _anonymizer.Anonymize(
                field.Value,
                [recognizerResult],
                new Dictionary<string, OperatorConfig>(StringComparer.Ordinal) { [entityType] = config });

            if (field.Parent is JsonObject objectParent)
            {
                objectParent[field.Name] = anonymized.Text;
            }
            else if (field.Parent is JsonArray arrayParent && field.ArrayIndex is { } index)
            {
                arrayParent[index] = anonymized.Text;
            }
        }

        return result;
    }

    private static IEnumerable<JsonStringField> EnumerateStringFields(JsonNode node, string path = "")
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var (name, child) in jsonObject)
                {
                    if (child is null)
                    {
                        continue;
                    }

                    var childPath = $"{path}/{EscapePointerSegment(name)}";
                    if (child is JsonValue childValue && childValue.TryGetValue<string>(out var text))
                    {
                        yield return new JsonStringField(childPath, text, jsonObject, name, null);
                        continue;
                    }

                    foreach (var field in EnumerateStringFields(child, childPath))
                    {
                        yield return field;
                    }
                }

                break;

            case JsonArray jsonArray:
                for (var index = 0; index < jsonArray.Count; index++)
                {
                    if (jsonArray[index] is not { } child)
                    {
                        continue;
                    }

                    var childPath = $"{path}/{index}";
                    if (child is JsonValue childValue && childValue.TryGetValue<string>(out var text))
                    {
                        yield return new JsonStringField(childPath, text, jsonArray, string.Empty, index);
                        continue;
                    }

                    foreach (var field in EnumerateStringFields(child, childPath))
                    {
                        yield return field;
                    }
                }

                break;

        }
    }

    private static void ValidateThreshold(double scoreThreshold)
    {
        if (scoreThreshold is < 0 or > 1 || double.IsNaN(scoreThreshold))
        {
            throw new ArgumentOutOfRangeException(nameof(scoreThreshold), "The score threshold must be between zero and one.");
        }
    }

    private static void ValidateRoot(JsonNode data)
    {
        if (data is not JsonObject && data is not JsonArray)
        {
            throw new NotSupportedException("Structured analysis supports JSON object and array roots only.");
        }
    }

    private static string EscapePointerSegment(string segment) => segment.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);

    private sealed record JsonStringField(string Path, string Value, JsonNode? Parent, string Name, int? ArrayIndex);
}
