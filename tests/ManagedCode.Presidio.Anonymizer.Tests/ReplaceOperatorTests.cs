using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class ReplaceOperatorTests
{
    [Fact]
    public void OperateUsesProvidedReplacement()
    {
        var op = new ReplaceOperator();
        var output = op.Operate(
            "secret",
            new Dictionary<string, object?>
            {
                [ReplaceOperator.NewValueKey] = "replacement",
                ["entity_type"] = "PERSON",
            });

        Assert.Equal("replacement", output);
    }

    [Fact]
    public void OperateFallsBackToEntityPlaceholder()
    {
        var op = new ReplaceOperator();
        var output = op.Operate(
            "secret",
            new Dictionary<string, object?>
            {
                ["entity_type"] = "PERSON",
            });

        Assert.Equal("<PERSON>", output);
    }

    [Fact]
    public void ValidateRejectsNonStringReplacement()
    {
        var op = new ReplaceOperator();
        var parameters = new Dictionary<string, object?>
        {
            [ReplaceOperator.NewValueKey] = 123,
        };

        var exception = Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
        Assert.Equal("Invalid parameter value for new_value. Expecting 'string', but got 'number'.", exception.Message);
    }
}
