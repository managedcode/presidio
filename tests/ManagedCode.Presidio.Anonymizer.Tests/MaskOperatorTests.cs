using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class MaskOperatorTests
{
    [Theory]
    [InlineData("123456789", 4, false, "*", "****56789")]
    [InlineData("123456789", 4, true, "*", "12345****")]
    [InlineData("123", 5, false, "#", "###")]
    [InlineData("123", 0, false, "*", "123")]
    public void MaskOperatorAppliesMask(string input, int charsToMask, bool fromEnd, string maskChar, string expected)
    {
        var op = new MaskOperator();
        var parameters = new Dictionary<string, object?>
        {
            [MaskOperator.CharsToMaskKey] = charsToMask,
            [MaskOperator.FromEndKey] = fromEnd,
            [MaskOperator.MaskingCharKey] = maskChar,
        };

        op.Validate(parameters);
        var output = op.Operate(input, parameters);

        Assert.Equal(expected, output);
    }

    [Fact]
    public void MaskOperatorRejectsInvalidMaskCharacter()
    {
        var op = new MaskOperator();
        var parameters = new Dictionary<string, object?>
        {
            [MaskOperator.CharsToMaskKey] = 4,
            [MaskOperator.FromEndKey] = false,
            [MaskOperator.MaskingCharKey] = "**",
        };

        var exception = Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
        Assert.Equal("Invalid input, masking_char must be a character", exception.Message);
    }

    [Fact]
    public void MaskOperatorRequiresMandatoryParameters()
    {
        var op = new MaskOperator();
        var parameters = new Dictionary<string, object?>
        {
            [MaskOperator.MaskingCharKey] = "*",
        };

        Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
    }
}
