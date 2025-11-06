using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class CustomOperatorTests
{
    [Fact]
    public void CustomOperatorReturnsLambdaResult()
    {
        var op = new CustomOperator();
        var parameters = new Dictionary<string, object?>
        {
            [CustomOperator.LambdaKey] = new Func<string, string>(value => value.ToUpperInvariant()),
        };

        op.Validate(parameters);
        var output = op.Operate("pii", parameters);

        Assert.Equal("PII", output);
    }

    [Fact]
    public void CustomOperatorRequiresCallable()
    {
        var op = new CustomOperator();
        var parameters = new Dictionary<string, object?>
        {
            [CustomOperator.LambdaKey] = "not a function",
        };

        var exception = Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
        Assert.Equal("New value must be a callable function", exception.Message);
    }

    [Fact]
    public void CustomOperatorRejectsNullReturnValue()
    {
        var op = new CustomOperator();
        var parameters = new Dictionary<string, object?>
        {
            [CustomOperator.LambdaKey] = new Func<string, string>(_ => null!),
        };

        var exception = Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
        Assert.Equal("Function return type must be a str", exception.Message);
    }
}
