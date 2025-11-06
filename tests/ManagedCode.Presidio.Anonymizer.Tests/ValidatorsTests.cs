using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class ValidatorsTests
{
    [Fact]
    public void ValidateParameterExistsAllowsZero()
    {
        Validators.ValidateParameterExists(0, "entity", "name");
        Validators.ValidateParameterExists(string.Empty, "entity", "name");
    }

    [Fact]
    public void ValidateParameterExistsThrowsWhenMissing()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameterExists(null, "entity", "name"));
        Assert.Equal("Invalid input, entity must contain name", exception.Message);
    }

    [Fact]
    public void ValidateParameterNotEmptyAllowsNonEmpty()
    {
        Validators.ValidateParameterNotEmpty("1234", "entity", "name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(0)]
    public void ValidateParameterNotEmptyRejectsEmptyValues(object? value)
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameterNotEmpty(value, "entity", "name"));
        Assert.Equal("Invalid input, entity must contain name", exception.Message);
    }

    [Fact]
    public void ValidateParameterInRangeThrowsWhenValueOutsideRange()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameterInRange(new[] { "0", "2" }, "1", "name", typeof(string)));
        Assert.Equal("Parameter name value 1 is not in range of values ['0', '2']", exception.Message);
    }

    [Fact]
    public void ValidateParameterInRangeAllowsValueInsideRange()
    {
        Validators.ValidateParameterInRange(new[] { "1", "2" }, "1", "name", typeof(string));
    }

    [Fact]
    public void ValidateParameterThrowsWhenValueMissing()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameter(null, "name", typeof(int)));
        Assert.Equal("Expected parameter name", exception.Message);
    }

    [Fact]
    public void ValidateParameterThrowsWhenTypeMismatch()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameter("1", "name", typeof(int)));
        Assert.Equal("Invalid parameter value for name. Expecting 'number', but got 'string'.", exception.Message);
    }

    [Fact]
    public void ValidateParameterProducesGenericMessageForUnknownType()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateParameter("1", "name", typeof(Tuple<int, int>)));
        Assert.Equal("Invalid parameter value for 'name'.", exception.Message);
    }

    [Fact]
    public void ValidateTypeAllowsNull()
    {
        Validators.ValidateType(null, "name", typeof(string));
    }

    [Fact]
    public void ValidateTypeRejectsWrongType()
    {
        var exception = Assert.Throws<InvalidParamException>(() => Validators.ValidateType(1, "name", typeof(string)));
        Assert.Equal("Invalid parameter value for name. Expecting 'string', but got 'number'.", exception.Message);
    }

    [Fact]
    public void ValidateTypeAllowsCorrectType()
    {
        Validators.ValidateType("1", "name", typeof(string));
    }
}
