namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Configuration describing the operator that should be used for anonymization.
/// </summary>
public sealed class OperatorConfig(string operatorName, IDictionary<string, object?>? parameters = null) : IEquatable<OperatorConfig>
{
    public string OperatorName { get; } = string.IsNullOrWhiteSpace(operatorName)
            ? throw new ArgumentException("Operator name must be provided.", nameof(operatorName))
            : operatorName;

    public IDictionary<string, object?> Parameters { get; } = parameters is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(parameters);

    public static OperatorConfig FromDictionary(IDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (!values.TryGetValue("type", out var typeValue) || typeValue is not string typeName)
        {
            throw new ArgumentException("Operator definition must contain a 'type' entry.", nameof(values));
        }

        var copied = new Dictionary<string, object?>(values);
        copied.Remove("type");
        return new OperatorConfig(typeName, copied);
    }

    public bool Equals(OperatorConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!OperatorName.Equals(other.OperatorName, StringComparison.Ordinal))
        {
            return false;
        }

        if (Parameters.Count != other.Parameters.Count)
        {
            return false;
        }

        foreach (var pair in Parameters)
        {
            if (!other.Parameters.TryGetValue(pair.Key, out var otherValue))
            {
                return false;
            }

            if (!Equals(pair.Value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as OperatorConfig);

    public override int GetHashCode() => HashCode.Combine(OperatorName);

    public override string ToString() => $"operator_name: {OperatorName}, params: {Parameters.Count}";
}
