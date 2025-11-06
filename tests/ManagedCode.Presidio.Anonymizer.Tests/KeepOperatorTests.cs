using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class KeepOperatorTests
{
    [Fact]
    public void OperateReturnsOriginalText()
    {
        var op = new KeepOperator();
        var output = op.Operate("original", new Dictionary<string, object?>());

        Assert.Equal("original", output);
    }

    [Fact]
    public void ValidateDoesNotThrow()
    {
        var op = new KeepOperator();

        var exception = Record.Exception(() => op.Validate(new Dictionary<string, object?>()));
        Assert.Null(exception);
    }
}
