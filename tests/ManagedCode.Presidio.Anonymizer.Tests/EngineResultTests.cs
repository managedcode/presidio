using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class EngineResultTests
{
    [Fact]
    public void DefaultConstructorLeavesTextNullAndItemsEmpty()
    {
        var result = new EngineResult();

        Assert.Null(result.Text);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void ParameterConstructorInitializesState()
    {
        var item = new OperatorResult(0, 3, "NAME", "***", "mask");
        var result = new EngineResult("masked", new List<OperatorResult> { item });

        Assert.Equal("masked", result.Text);
        Assert.Same(item, result.Items.Single());
    }

    [Fact]
    public void NormalizeItemIndexesConvertsFromEndOffsets()
    {
        var item = new OperatorResult(1, 2, "PERSON", "XX", "mask");
        var result = new EngineResult("*****", new List<OperatorResult> { item });

        result.NormalizeItemIndexes();

        Assert.Equal(3, result.Items[0].Start);
        Assert.Equal(5, result.Items[0].End);
    }

    [Fact]
    public void AddItemAppendsToCollection()
    {
        var result = new EngineResult();
        var item = new OperatorResult(0, 4, "PHONE_NUMBER", "****", "mask");

        result.AddItem(item);

        Assert.Single(result.Items);
        Assert.Same(item, result.Items[0]);
    }

    [Fact]
    public void SetTextUpdatesValue()
    {
        var result = new EngineResult();

        result.SetText("updated");

        Assert.Equal("updated", result.Text);
    }

    [Fact]
    public void EqualityDependsOnTextAndItems()
    {
        var left = new EngineResult("masked", new List<OperatorResult>
        {
            new(0, 4, "NAME", "****", "mask"),
        });

        var right = new EngineResult("masked", new List<OperatorResult>
        {
            new(0, 4, "NAME", "****", "mask"),
        });

        Assert.Equal(left, right);
        Assert.True(left.Equals(right));

        var different = new EngineResult("other", new List<OperatorResult>());
        Assert.NotEqual(left, different);
        Assert.False(left.Equals(different));
    }

    [Fact]
    public void ToJsonSerializesTextAndItems()
    {
        var result = new EngineResult("masked", new List<OperatorResult>
        {
            new(0, 4, "NAME", "****", "mask"),
        });

        var json = result.ToJson();

        Assert.Contains("\"text\":\"masked\"", json, StringComparison.Ordinal);
        Assert.Contains("\"entity_type\":\"NAME\"", json, StringComparison.Ordinal);
    }
}
