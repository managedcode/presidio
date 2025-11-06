namespace ManagedCode.Presidio.Anonymizer;

public sealed class RedactOperator : Operator
{
    public override string Operate(string text, IDictionary<string, object?>? parameters = null) => string.Empty;

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        // No validation required.
    }

    public override string OperatorName => "redact";

    public override OperatorType Type => OperatorType.Anonymize;
}
