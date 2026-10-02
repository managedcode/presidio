using System.Collections.ObjectModel;

namespace ManagedCode.Presidio.Structured;

/// <summary>Maps RFC 6901 JSON Pointer paths to the PII entity type found in their values.</summary>
public sealed class StructuredAnalysis
{
    public StructuredAnalysis(IReadOnlyDictionary<string, string> entityMapping)
    {
        ArgumentNullException.ThrowIfNull(entityMapping);
        if (entityMapping.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value)))
        {
            throw new ArgumentException("Every structured entity mapping requires a path and entity type.", nameof(entityMapping));
        }

        EntityMapping = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(entityMapping, StringComparer.Ordinal));
    }

    public IReadOnlyDictionary<string, string> EntityMapping { get; }
}
