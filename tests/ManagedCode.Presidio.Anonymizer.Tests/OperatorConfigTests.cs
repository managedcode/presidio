using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class OperatorConfigTests
{
    [Fact]
    public void OperatorConfigEqualityUsesNameAndParameters()
    {
        var first = new OperatorConfig("mask", new Dictionary<string, object?> { ["char"] = "*" });
        var second = new OperatorConfig("mask", new Dictionary<string, object?> { ["char"] = "*" });
        var third = new OperatorConfig("hash", new Dictionary<string, object?> { ["seed"] = 42 });

        Assert.Equal(first, second);
        Assert.NotEqual(first, third);
        Assert.True(first.Equals(second));
        Assert.False(first.Equals(third));
    }

    [Fact]
    public void FromDictionaryExtractsTypeAndParameters()
    {
        var source = new Dictionary<string, object?>
        {
            { "type", "mask" },
            { "masking_char", "*" },
        };

        var config = OperatorConfig.FromDictionary(source);

        Assert.Equal("mask", config.OperatorName);
        Assert.Single(config.Parameters);
        Assert.Equal("*", config.Parameters["masking_char"]);
    }

    [Fact]
    public void OperatorResultCapturesSpanAndMetadata()
    {
        var result = new OperatorResult(0, 4, "PERSON", "****", "mask");

        Assert.Equal(0, result.Start);
        Assert.Equal(4, result.End);
        Assert.Equal("****", result.Text);
        Assert.Equal("mask", result.Operator);
        Assert.Equal("PERSON", result.ToDictionary()["entity_type"]);
    }

    [Fact]
    public void EngineResultAggregatesItems()
    {
        var result = new EngineResult();
        result.SetText("redacted");

        result.AddItem(new OperatorResult(0, 4, "PERSON", "****", "mask"));
        result.AddItem(new OperatorResult(10, 20, "EMAIL", "[email]", "replace"));

        Assert.Equal("redacted", result.Text);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains("\"EMAIL\"", result.ToJson(), StringComparison.Ordinal);
    }

    [Fact]
    public void PiiEntityComparisonOrdersBySpan()
    {
        var earlier = new TestEntity(0, 4, "PERSON");
        var later = new TestEntity(5, 8, "PERSON");

        Assert.True(earlier < later);
        Assert.True(later > earlier);
    }

    private sealed class TestEntity(int start, int end, string entityType) : PiiEntity(start, end, entityType)
    {
    }
}
