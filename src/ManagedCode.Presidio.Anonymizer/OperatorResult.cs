using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Result emitted by an anonymization operator containing the replacement text and metadata.
/// Mirrors the mutable semantics of the Python implementation.
/// </summary>
public sealed class OperatorResult(int start, int end, string entityType, string? text = null, string? operatorName = null) : PiiEntity(start, end, entityType), IComparable<OperatorResult>, IEquatable<OperatorResult>
{
    [JsonPropertyName("text")]
    public string? Text { get; } = text;

    [JsonPropertyName("operator")]
    public string? Operator { get; } = operatorName;

    public int CompareTo(OperatorResult? other) => base.CompareTo(other);

    public bool Equals(OperatorResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!base.Equals(other))
        {
            return false;
        }

        return string.Equals(Text, other.Text, StringComparison.Ordinal)
               && string.Equals(Operator, other.Operator, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as OperatorResult);

    public override int GetHashCode() => HashCode.Combine(Start, End, EntityType, Text, Operator);

    public override string ToString() => JsonSerializer.Serialize(ToDictionary());

    public IReadOnlyDictionary<string, object?> ToDictionary() => new Dictionary<string, object?>
    {
        ["start"] = Start,
        ["end"] = End,
        ["entity_type"] = EntityType,
        ["text"] = Text,
        ["operator"] = Operator,
    };
}
