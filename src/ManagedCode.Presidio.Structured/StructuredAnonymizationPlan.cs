using System.Collections.Immutable;

namespace ManagedCode.Presidio.Structured;

/// <summary>
/// Declares how each structured field should be anonymized.
/// </summary>
public sealed class StructuredAnonymizationPlan
{
    private readonly ImmutableDictionary<string, string> _operatorByPath;

    private StructuredAnonymizationPlan(ImmutableDictionary<string, string> operatorByPath)
    {
        _operatorByPath = operatorByPath;
    }

    public IReadOnlyDictionary<string, string> OperatorByPath => _operatorByPath;

    public bool TryGetOperator(string path, out string? operatorId) =>
        _operatorByPath.TryGetValue(path, out operatorId);

    public static Builder CreateBuilder() => new();

    public sealed class Builder
    {
        private readonly Dictionary<string, string> _entries = new(StringComparer.Ordinal);

        public Builder Add(string path, string operatorId)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path must be provided.", nameof(path));
            }

            if (string.IsNullOrWhiteSpace(operatorId))
            {
                throw new ArgumentException("Operator identifier must be provided.", nameof(operatorId));
            }

            _entries[path] = operatorId;
            return this;
        }

        public StructuredAnonymizationPlan Build() =>
            new(ImmutableDictionary.CreateRange(StringComparer.Ordinal, _entries));
    }
}
