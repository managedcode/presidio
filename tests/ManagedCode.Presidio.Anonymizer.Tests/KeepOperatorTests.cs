using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class KeepOperatorTests
{
    [Fact]
    public void OperateReturnsOriginalText()
    {
        var output = new KeepOperator().Operate("original", new Dictionary<string, object?>());

        Assert.Equal("original", output);
    }

    [Fact]
    public void DeanonymizeVariantAlsoReturnsOriginalText()
    {
        var output = new DeanonymizeKeepOperator().Operate("original", new Dictionary<string, object?>());

        Assert.Equal("original", output);
    }

    [Fact]
    public void ValidateDoesNotThrow()
    {
        Assert.Null(Record.Exception(() => new KeepOperator().Validate(new Dictionary<string, object?>())));
        Assert.Null(Record.Exception(() => new DeanonymizeKeepOperator().Validate(new Dictionary<string, object?>())));
    }

    [Fact]
    public void OperatorNamesMatchPythonBehaviour()
    {
        Assert.Equal("keep", new KeepOperator().OperatorName);
        Assert.Equal("deanonymize_keep", new DeanonymizeKeepOperator().OperatorName);
    }
}
