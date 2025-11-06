using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class OperatorResultTests
{
    [Fact]
    public void ConstructorAssignsProperties()
    {
        var result = new OperatorResult(0, 3, "NAME", "abc", "mask");

        Assert.Equal(0, result.Start);
        Assert.Equal(3, result.End);
        Assert.Equal("NAME", result.EntityType);
        Assert.Equal("abc", result.Text);
        Assert.Equal("mask", result.Operator);
    }

    [Fact]
    public void EqualityConsidersSpanEntityTextAndOperator()
    {
        var first = new OperatorResult(0, 3, "NAME", "abc", "mask");
        var second = new OperatorResult(0, 3, "NAME", "abc", "mask");
        var differentText = new OperatorResult(0, 3, "NAME", "def", "mask");
        var differentOperator = new OperatorResult(0, 3, "NAME", "abc", "replace");
        var differentSpan = new OperatorResult(1, 4, "NAME", "abc", "mask");

        Assert.Equal(first, second);
        Assert.NotEqual(first, differentText);
        Assert.NotEqual(first, differentOperator);
        Assert.NotEqual(first, differentSpan);
    }

    [Fact]
    public void ToDictionaryProducesSerializableShape()
    {
        var result = new OperatorResult(0, 3, "NAME", "abc", "mask");
        var payload = result.ToDictionary();

        Assert.Equal(0, payload["start"]);
        Assert.Equal(3, payload["end"]);
        Assert.Equal("NAME", payload["entity_type"]);
        Assert.Equal("abc", payload["text"]);
        Assert.Equal("mask", payload["operator"]);
    }
}
