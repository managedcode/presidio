using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class ReplaceOperatorTests
{
    [Fact]
    public void OperateUsesProvidedReplacement()
    {
        var output = new ReplaceOperator().Operate(
            "secret",
            new Dictionary<string, object?>
            {
                [ReplaceOperator.NewValueKey] = "replacement",
                ["entity_type"] = "PERSON",
            });

        Assert.Equal("replacement", output);
    }

    [Theory]
    [InlineData("", "<>")]
    [InlineData("PHONE_NUMBER", "<PHONE_NUMBER>")]
    public void OperateFallsBackToEntityPlaceholder(string entityType, string expected)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["entity_type"] = entityType,
        };

        var output = new ReplaceOperator().Operate("secret", parameters);

        Assert.Equal(expected, output);
    }

    [Fact]
    public void ValidateRejectsNonStringReplacement()
    {
        var parameters = new Dictionary<string, object?>
        {
            [ReplaceOperator.NewValueKey] = 123,
        };

        var exception = Assert.Throws<InvalidParamException>(() => new ReplaceOperator().Validate(parameters));
        Assert.Equal("Invalid parameter value for new_value. Expecting 'string', but got 'number'.", exception.Message);
    }

    [Fact]
    public void OperatorNameIsReplace()
    {
        Assert.Equal("replace", new ReplaceOperator().OperatorName);
    }
}
