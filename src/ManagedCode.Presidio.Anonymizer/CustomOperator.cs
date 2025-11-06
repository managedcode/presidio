namespace ManagedCode.Presidio.Anonymizer;

public sealed class CustomOperator : Operator
{
    public const string LambdaKey = "lambda";

    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var lambda = ResolveLambda(parameters);
        return lambda(text ?? string.Empty);
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var lambda = ResolveLambda(parameters);

        _ = lambda("PII") ?? throw new InvalidParamException("Function return type must be a str");
    }

    public override string OperatorName => "custom";

    public override OperatorType Type => OperatorType.Anonymize;

    private static Func<string, string> ResolveLambda(IDictionary<string, object?>? parameters)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        if (!effectiveParams.TryGetValue(LambdaKey, out var value) || value is not Func<string, string> lambda)
        {
            throw new InvalidParamException("New value must be a callable function");
        }

        return lambda;
    }
}
