using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class RedactOperatorTests
{
    [Fact]
    public void OperateReturnsEmptyString()
    {
        var op = new RedactOperator();
        var output = op.Operate("secret", new Dictionary<string, object?>());

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void ValidateDoesNothing()
    {
        var op = new RedactOperator();
        var exception = Record.Exception(() => op.Validate(new Dictionary<string, object?>()));

        Assert.Null(exception);
    }
}
