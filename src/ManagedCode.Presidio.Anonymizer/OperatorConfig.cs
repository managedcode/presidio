using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Configuration describing the operator that should be used for anonymization.
/// Mirrors the Python OperatorConfig behaviour.
/// </summary>
public sealed class OperatorConfig : IEquatable<OperatorConfig>
{
    public OperatorConfig(string operatorName, IDictionary<string, object?>? parameters = null)
    {
        Validators.ValidateParameterNotEmpty(operatorName, "operator config", "operator_name");

        OperatorName = operatorName;
        Parameters = parameters is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(parameters);
    }

    public string OperatorName { get; }

    public IDictionary<string, object?> Parameters { get; }

    public static OperatorConfig FromDictionary(IDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (!values.TryGetValue("type", out var typeValue) || typeValue is not string operatorName || string.IsNullOrWhiteSpace(operatorName))
        {
            throw new InvalidParamException("Operator definition must contain a 'type' entry.");
        }

        var copied = new Dictionary<string, object?>(values, StringComparer.Ordinal);
        copied.Remove("type");
        return new OperatorConfig(operatorName, copied);
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

    public override int GetHashCode() => HashCode.Combine(OperatorName, Parameters.Count);

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("operator_name: ").Append(OperatorName).Append(", params: {");

        var index = 0;
        foreach (var pair in Parameters)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(pair.Key).Append(": ").Append(pair.Value);
            index++;
        }

        builder.Append('}');
        return builder.ToString();
    }
}
