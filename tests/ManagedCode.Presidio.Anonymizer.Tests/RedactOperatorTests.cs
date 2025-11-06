using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class RedactOperatorTests
{
    [Fact]
    public void OperateReturnsEmptyStringRegardlessOfParameters()
    {
        var output = new RedactOperator().Operate("secret", new Dictionary<string, object?>());
        Assert.Equal(string.Empty, output);

        output = new RedactOperator().Operate("secret", new Dictionary<string, object?> { ["new_value"] = string.Empty });
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void ValidateDoesNothing()
    {
        Assert.Null(Record.Exception(() => new RedactOperator().Validate(new Dictionary<string, object?>())));
    }

    [Fact]
    public void OperatorNameIsRedact()
    {
        Assert.Equal("redact", new RedactOperator().OperatorName);
    }
}
