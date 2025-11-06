namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Operator type either anonymize or deanonymize to separate operator categories.
/// </summary>
public enum OperatorType
{
    Anonymize,
    Deanonymize,
}

/// <summary>
/// Base abstraction for anonymizer operators.
/// </summary>
public abstract class Operator
{
    public abstract string Operate(string text, IDictionary<string, object?>? parameters = null);

    public abstract void Validate(IDictionary<string, object?>? parameters = null);

    public abstract string OperatorName { get; }

    public abstract OperatorType Type { get; }
}
