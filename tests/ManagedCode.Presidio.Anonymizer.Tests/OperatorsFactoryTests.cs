using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class OperatorsFactoryTests
{
    [Fact]
    public void FactoryCreatesRegisteredOperator()
    {
        var factory = new OperatorsFactory();
        var op = factory.CreateOperator("mask", OperatorType.Anonymize);

        Assert.IsType<MaskOperator>(op);
    }

    [Fact]
    public void FactoryRejectsUnknownOperator()
    {
        var factory = new OperatorsFactory();
        var exception = Assert.Throws<InvalidParamException>(() => factory.CreateOperator("unknown", OperatorType.Anonymize));
        Assert.Equal("Invalid operator class 'unknown'.", exception.Message);
    }

    [Fact]
    public void RemoveOperatorValidatesPresence()
    {
        var factory = new OperatorsFactory();
        var exception = Assert.Throws<InvalidParamException>(() => factory.RemoveAnonymizeOperator(typeof(DecryptOperator)));
        Assert.Equal("Operator decrypt not found in operators list", exception.Message);
    }

    [Fact]
    public void AddOperatorRequiresMatchingType()
    {
        var factory = new OperatorsFactory();
        var exception = Assert.Throws<InvalidParamException>(() => factory.AddAnonymizeOperator(typeof(DecryptOperator)));
        Assert.Equal("Operator decrypt does not match expected type 'Anonymize'.", exception.Message);
    }
}
