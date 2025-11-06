namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Executes deanonymization operations using registered operators.
/// Mirrors the behaviour of the Python DeanonymizeEngine.
/// </summary>
public sealed class DeanonymizeEngine : EngineBase
{
    public EngineResult Deanonymize(
        string text,
        IReadOnlyCollection<OperatorResult> entities,
        IDictionary<string, OperatorConfig> operators)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(operators);

        return Operate(text, entities, operators, OperatorType.Deanonymize);
    }

    public IReadOnlyCollection<string> GetDeanonymizers() => OperatorsFactory.GetDeanonymizers().Keys.ToArray();

    public void AddDeanonymizer(Type operatorType) => OperatorsFactory.AddDeanonymizeOperator(operatorType);

    public void RemoveDeanonymizer(Type operatorType) => OperatorsFactory.RemoveDeanonymizeOperator(operatorType);
}
