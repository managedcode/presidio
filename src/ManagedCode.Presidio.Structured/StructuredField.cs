using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Structured;

/// <summary>
/// Represents a single field within a structured payload (JSON record, CSV column, etc.).
/// </summary>
public sealed class StructuredField(string path, string value, RecognizerResult? detection = null)
{
    public string Path { get; } = !string.IsNullOrWhiteSpace(path)
            ? path
            : throw new ArgumentException("Field path must be provided.", nameof(path));

    public string Value { get; } = value ?? string.Empty;

    public RecognizerResult? Detection { get; } = detection;

    public StructuredField WithDetection(RecognizerResult detection) =>
        new(Path, Value, detection);
}
