using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Result emitted by an anonymization operator containing the replacement text and metadata.
/// </summary>
public sealed class OperatorResult(TextSpan span, string entityType, string? text = null, string? operatorName = null) : PiiEntity(span, entityType)
{
    public string? Text { get; } = text;

    public string? Operator { get; } = operatorName;

    public IReadOnlyDictionary<string, object?> ToDictionary() =>
        new Dictionary<string, object?>
        {
            { nameof(Start), Start },
            { nameof(End), End },
            { nameof(EntityType), EntityType },
            { nameof(Text), Text },
            { nameof(Operator), Operator },
        };
}
