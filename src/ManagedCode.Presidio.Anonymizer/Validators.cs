using System.Collections;
using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Validation helpers mirroring the Python validators used by Presidio anonymizer.
/// </summary>
internal static class Validators
{
    private static readonly IReadOnlyDictionary<Type, string> TypeDisplayNames = new Dictionary<Type, string>
    {
        [typeof(string)] = "string",
        [typeof(bool)] = "boolean",
        [typeof(int)] = "number",
        [typeof(long)] = "number",
        [typeof(double)] = "number",
        [typeof(float)] = "number",
        [typeof(decimal)] = "number",
        [typeof(IList)] = "array",
        [typeof(Array)] = "array",
        [typeof(object)] = "object",
    };

    public static void ValidateParameterInRange<T>(IEnumerable<T> valuesRange, T parameterValue, string parameterName, Type parameterType)
    {
        ValidateParameter(parameterValue, parameterName, typeof(object));

        if (!valuesRange.Contains(parameterValue))
        {
            throw new InvalidParamException(
                $"Parameter {parameterName} value {parameterValue} is not in range of values {FormatValues(valuesRange)}");
        }
    }

    public static void ValidateParameterNotEmpty(object? parameterValue, string entity, string parameterName)
    {
        if (!IsTruthy(parameterValue))
        {
            throw new InvalidParamException($"Invalid input, {entity} must contain {parameterName}");
        }
    }

    public static void ValidateParameterExists(object? parameterValue, string entity, string parameterName)
    {
        if (parameterValue is null)
        {
            throw new InvalidParamException($"Invalid input, {entity} must contain {parameterName}");
        }
    }

    public static void ValidateParameter(object? parameterValue, string parameterName, Type parameterType)
    {
        if (parameterValue is null)
        {
            throw new InvalidParamException($"Expected parameter {parameterName}");
        }

        ValidateType(parameterValue, parameterName, parameterType);
    }

    public static void ValidateType(object? parameterValue, string parameterName, Type parameterType)
    {
        if (parameterValue is null)
        {
            return;
        }

        if (!parameterType.IsInstanceOfType(parameterValue))
        {
            throw new InvalidParamException(BuildBadTypedParameterErrorMessage(parameterName, parameterType, parameterValue.GetType()));
        }
    }

    private static bool IsTruthy(object? value)
    {
        if (value is null)
        {
            return false;
        }

        return value switch
        {
            string s => !string.IsNullOrEmpty(s),
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => Math.Abs(d) > double.Epsilon,
            float f => Math.Abs(f) > float.Epsilon,
            decimal m => m != 0,
            IEnumerable enumerable => HasAny(enumerable),
            _ => true,
        };
    }

    private static string FormatValues<T>(IEnumerable<T> values)
    {
        var builder = new StringBuilder("[");
        var first = true;
        foreach (var value in values)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            if (value is string s)
            {
                builder.Append('\'').Append(s).Append('\'');
            }
            else
            {
                builder.Append(value);
            }
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }

    private static string BuildBadTypedParameterErrorMessage(string parameterName, Type expectedType, Type actualType)
    {
        var expected = ResolveDisplayName(expectedType);
        var actual = ResolveDisplayName(actualType);

        if (!string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(actual))
        {
            return $"Invalid parameter value for {parameterName}. Expecting '{expected}', but got '{actual}'.";
        }

        return $"Invalid parameter value for '{parameterName}'.";
    }

    private static string ResolveDisplayName(Type type)
    {
        if (TypeDisplayNames.TryGetValue(type, out var name))
        {
            return name;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return "array";
        }

        return string.Empty;
    }

    private static bool HasAny(IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        try
        {
            return enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
