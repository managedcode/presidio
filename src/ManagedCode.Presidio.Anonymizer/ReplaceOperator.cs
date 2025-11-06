using System.Globalization;

namespace ManagedCode.Presidio.Anonymizer;

public sealed class ReplaceOperator : Operator
{
    public const string NewValueKey = "new_value";

    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        if (!effectiveParams.TryGetValue(NewValueKey, out var newValue) || IsNullOrEmpty(newValue))
        {
            var entityType = effectiveParams.TryGetValue("entity_type", out var entityTypeValue)
                ? entityTypeValue?.ToString()
                : string.Empty;
            return $"<{entityType}>";
        }

        return ConvertToString(newValue);
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        if (effectiveParams.TryGetValue(NewValueKey, out var newValue))
        {
            Validators.ValidateType(newValue, NewValueKey, typeof(string));
        }
    }

    public override string OperatorName => "replace";

    public override OperatorType Type => OperatorType.Anonymize;

    private static string ConvertToString(object value) => value switch
    {
        string text => text,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static bool IsNullOrEmpty(object? value) => value switch
    {
        null => true,
        string s => string.IsNullOrEmpty(s),
        _ => false,
    };
}
