using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Container for anonymization engine results.
/// </summary>
public sealed class EngineResult(string? text = null, IList<OperatorResult>? items = null) : IEquatable<EngineResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string? Text { get; private set; } = text;

    public IList<OperatorResult> Items { get; } = items ?? new List<OperatorResult>();

    public void SetText(string text) => Text = text;

    public void AddItem(OperatorResult item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Items.Add(item);
    }

    public void NormalizeItemIndexes()
    {
        var textLength = Text?.Length ?? 0;
        foreach (var item in Items)
        {
            var normalizedStart = textLength - item.End;
            var normalizedEnd = normalizedStart + (item.Text?.Length ?? 0);
            item.UpdateSpan(normalizedStart, normalizedEnd);
        }
    }

    public string ToJson()
    {
        var payload = new
        {
            text = Text,
            items = Items.Select(item => item.ToDictionary()).ToArray(),
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public override string ToString()
    {
        if (Items.Count == 0)
        {
            return $"text: {Text}\nitems:\n[\n\n]\n";
        }

        var builder = new StringBuilder();
        builder.Append("text: ").Append(Text).Append('\n');
        builder.Append("items:\n[\n");
        for (var i = 0; i < Items.Count; i++)
        {
            builder.Append("    ").Append(Items[i]).Append('\n');
        }

        builder.Append(']').Append('\n');
        return builder.ToString();
    }

    public bool Equals(EngineResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Text, other.Text, StringComparison.Ordinal)
               && Items.SequenceEqual(other.Items);
    }

    public override bool Equals(object? obj) => Equals(obj as EngineResult);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Text, StringComparer.Ordinal);
        foreach (var item in Items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}
