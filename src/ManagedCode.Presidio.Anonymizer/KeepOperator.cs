namespace ManagedCode.Presidio.Anonymizer;

public abstract class BaseKeepOperator : Operator
{
    public override string Operate(string text, IDictionary<string, object?>? parameters = null) => text;

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        // No validation required for keep operators.
    }
}

public sealed class KeepOperator : BaseKeepOperator
{
    public override string OperatorName => "keep";

    public override OperatorType Type => OperatorType.Anonymize;
}

public sealed class DeanonymizeKeepOperator : BaseKeepOperator
{
    public override string OperatorName => "deanonymize_keep";

    public override OperatorType Type => OperatorType.Deanonymize;
}
