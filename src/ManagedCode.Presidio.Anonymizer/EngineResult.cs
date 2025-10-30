using System.Text.Json;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Container for anonymization engine results.
/// </summary>
public sealed class EngineResult(string? text = null, IList<OperatorResult>? items = null)
{
    public string? Text { get; private set; } = text;

    public IList<OperatorResult> Items { get; } = items ?? new List<OperatorResult>();

    public void SetText(string text) => Text = text;

    public void AddItem(OperatorResult item) => Items.Add(item);

    public string ToJson() => JsonSerializer.Serialize(this);

    public override string ToString() => $"text: {Text}\nitems: {Items.Count}";
}
