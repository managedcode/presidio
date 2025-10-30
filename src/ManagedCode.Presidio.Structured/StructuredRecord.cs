using System.Collections.ObjectModel;
using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Structured;

/// <summary>
/// Represents a flattened view of a structured document (e.g., JSON keyed by JSONPath).
/// </summary>
public sealed class StructuredRecord
{
    private readonly ReadOnlyDictionary<string, StructuredField> _fields;

    public StructuredRecord(IEnumerable<KeyValuePair<string, string>> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        var map = new Dictionary<string, StructuredField>(StringComparer.Ordinal);
        foreach (var (path, value) in fields)
        {
            var field = new StructuredField(path, value);
            map[path] = field;
        }

        _fields = new ReadOnlyDictionary<string, StructuredField>(map);
        Fields = new ReadOnlyCollection<StructuredField>(map.Values.ToList());
    }

    private StructuredRecord(IEnumerable<KeyValuePair<string, StructuredField>> fields)
    {
        var map = fields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        _fields = new ReadOnlyDictionary<string, StructuredField>(map);
        Fields = new ReadOnlyCollection<StructuredField>(map.Values.ToList());
    }

    public IReadOnlyList<StructuredField> Fields { get; }

    public bool TryGetField(string path, out StructuredField field) =>
        _fields.TryGetValue(path, out field!);

    public StructuredRecord WithDetections(IEnumerable<RecognizerResult> detections, Func<RecognizerResult, string> pathSelector)
    {
        ArgumentNullException.ThrowIfNull(detections);

        ArgumentNullException.ThrowIfNull(pathSelector);

        var builder = new Dictionary<string, StructuredField>(_fields, StringComparer.Ordinal);

        foreach (var detection in detections)
        {
            var path = pathSelector(detection);
            if (builder.TryGetValue(path, out var existing))
            {
                builder[path] = existing.WithDetection(detection);
            }
        }

        return new StructuredRecord(builder);
    }

    public StructuredRecord UpdateField(string path, string newValue)
    {
        if (!_fields.TryGetValue(path, out var existing))
        {
            throw new KeyNotFoundException($"Field '{path}' was not found in the structured record.");
        }

        var builder = new Dictionary<string, StructuredField>(_fields, StringComparer.Ordinal)
        {
            [path] = new StructuredField(path, newValue, existing.Detection),
        };

        return new StructuredRecord(builder);
    }
}
