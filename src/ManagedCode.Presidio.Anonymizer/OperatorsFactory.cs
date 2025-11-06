namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Factory that resolves operator identifiers to concrete implementations.
/// </summary>
public sealed class OperatorsFactory
{
    private static readonly Type[] PredefinedAnonymizers =
    {
        typeof(CustomOperator),
        typeof(EncryptOperator),
        typeof(HashOperator),
        typeof(KeepOperator),
        typeof(MaskOperator),
        typeof(RedactOperator),
        typeof(ReplaceOperator),
    };

    private static readonly Type[] PredefinedDeanonymizers =
    {
        typeof(DecryptOperator),
        typeof(DeanonymizeKeepOperator),
    };

    private readonly Dictionary<string, Type> _anonymizers;
    private readonly Dictionary<string, Type> _deanonymizers;

    public OperatorsFactory()
    {
        _anonymizers = LoadPredefined(PredefinedAnonymizers);
        _deanonymizers = LoadPredefined(PredefinedDeanonymizers);
    }

    public IReadOnlyDictionary<string, Type> GetAnonymizers() => new Dictionary<string, Type>(_anonymizers, StringComparer.Ordinal);

    public IReadOnlyDictionary<string, Type> GetDeanonymizers() => new Dictionary<string, Type>(_deanonymizers, StringComparer.Ordinal);

    public void AddAnonymizeOperator(Type operatorType) => AddOperator(operatorType, _anonymizers, OperatorType.Anonymize);

    public void AddDeanonymizeOperator(Type operatorType) => AddOperator(operatorType, _deanonymizers, OperatorType.Deanonymize);

    public void RemoveAnonymizeOperator(Type operatorType) => RemoveOperator(operatorType, _anonymizers);

    public void RemoveDeanonymizeOperator(Type operatorType) => RemoveOperator(operatorType, _deanonymizers);

    public Operator CreateOperator(string operatorName, OperatorType operatorType)
    {
        var dictionary = operatorType == OperatorType.Anonymize ? _anonymizers : _deanonymizers;
        if (!dictionary.TryGetValue(operatorName, out var type))
        {
            throw new InvalidParamException($"Invalid operator class '{operatorName}'.");
        }

        return (Operator)Activator.CreateInstance(type)!;
    }

    private static Dictionary<string, Type> LoadPredefined(IEnumerable<Type> operators)
    {
        var map = new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (var operatorType in operators)
        {
            var instance = (Operator)Activator.CreateInstance(operatorType)!;
            map[instance.OperatorName] = operatorType;
        }

        return map;
    }

    private static void AddOperator(Type operatorType, IDictionary<string, Type> target, OperatorType expectedType)
    {
        ArgumentNullException.ThrowIfNull(operatorType);
        if (!typeof(Operator).IsAssignableFrom(operatorType))
        {
            throw new ArgumentException($"Type '{operatorType}' must inherit from {nameof(Operator)}.", nameof(operatorType));
        }

        var instance = (Operator)Activator.CreateInstance(operatorType)!;
        if (instance.Type != expectedType)
        {
            throw new InvalidParamException($"Operator {instance.OperatorName} does not match expected type '{expectedType}'.");
        }

        target[instance.OperatorName] = operatorType;
    }

    private static void RemoveOperator(Type operatorType, IDictionary<string, Type> target)
    {
        ArgumentNullException.ThrowIfNull(operatorType);
        var instance = (Operator)Activator.CreateInstance(operatorType)!;
        if (!target.Remove(instance.OperatorName))
        {
            throw new InvalidParamException($"Operator {instance.OperatorName} not found in operators list");
        }
    }
}
