using Xunit;

namespace ManagedCode.Presidio.Core.Tests;

public sealed class RecognizerResultTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(2, 8)]
    [InlineData(0, 8)]
    public void ContainsReturnsTrueWhenSecondSpanIsWithinFirst(int start, int end)
    {
        var first = CreateResult("ENTITY", 0.5, 0, 10);
        var second = CreateResult("ENTITY", 0.5, start, end);

        Assert.True(first.Contains(second));
    }

    [Theory]
    [InlineData(4, 10)]
    [InlineData(5, 11)]
    [InlineData(0, 5)]
    [InlineData(0, 6)]
    public void ContainsReturnsFalseWhenSpansDoNotNest(int start, int end)
    {
        var first = CreateResult("ENTITY", 0.5, 5, 10);
        var second = CreateResult("ENTITY", 0.5, start, end);

        Assert.False(first.Contains(second));
    }

    [Fact]
    public void EqualIndicesReturnsTrueForMatchingSpans()
    {
        var first = CreateResult("ENTITY", 0.5, 0, 10);
        var second = CreateResult("ENTITY", 0.5, 0, 10);

        Assert.True(first.HasEqualIndices(second));
        Assert.True(first.EqualIndices(second));
    }

    [Theory]
    [InlineData(4, 10)]
    [InlineData(5, 11)]
    [InlineData(0, 5)]
    [InlineData(0, 6)]
    public void EqualIndicesReturnsFalseForDifferentSpans(int start, int end)
    {
        var first = CreateResult("ENTITY", 0.5, 5, 10);
        var second = CreateResult("ENTITY", 0.5, start, end);

        Assert.False(first.HasEqualIndices(second));
        Assert.False(first.EqualIndices(second));
    }

    [Fact]
    public void EqualsReturnsTrueForIdenticalResults()
    {
        var first = CreateResult("ENTITY", 0.2, 0, 10);
        var second = CreateResult("ENTITY", 0.2, 0, 10);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Theory]
    [InlineData("ENTITY_ALT", 0.2, 0, 10)]
    [InlineData("ENTITY", 0.3, 0, 10)]
    [InlineData("ENTITY", 0.2, 4, 10)]
    [InlineData("ENTITY", 0.2, 0, 11)]
    public void EqualsReturnsFalseForDifferentResults(string entityType, double score, int start, int end)
    {
        var first = CreateResult("ENTITY", 0.2, 0, 10);
        var second = CreateResult(entityType, score, start, end);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void GetHashCodeReturnsSameValueForIdenticalResults()
    {
        var first = CreateResult("ENTITY", 0.2, 0, 10);
        var second = CreateResult("ENTITY", 0.2, 0, 10);

        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [InlineData("ENTITY_ALT", 0.2, 0, 10)]
    [InlineData("ENTITY", 0.3, 0, 10)]
    [InlineData("ENTITY", 0.2, 4, 10)]
    [InlineData("ENTITY", 0.2, 0, 11)]
    public void GetHashCodeReturnsDifferentValueForDifferentResults(string entityType, double score, int start, int end)
    {
        var first = CreateResult("ENTITY", 0.2, 0, 10);
        var second = CreateResult(entityType, score, start, end);

        Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [InlineData("ENTITY", 0.2, 0, 10)]
    [InlineData("ENTITY_ALT", 0.2, 2, 10)]
    [InlineData("ENTITY", 0.3, 0, 11)]
    [InlineData("ENTITY", 0.1, 0, 10)]
    public void HasConflictReturnsTrueWhenSpansOverlapOrScoreLower(string entityType, double score, int start, int end)
    {
        var first = CreateResult("ENTITY", 0.2, 2, 10);
        var second = CreateResult(entityType, score, start, end);

        Assert.True(first.HasConflict(second));
    }

    [Theory]
    [InlineData("ENTITY", 0.2, 3, 10)]
    [InlineData("ENTITY_ALT", 0.1, 2, 10)]
    [InlineData("ENTITY", 0.3, 0, 9)]
    public void HasConflictReturnsFalseWhenSpansDoNotConflict(string entityType, double score, int start, int end)
    {
        var first = CreateResult("ENTITY", 0.2, 2, 10);
        var second = CreateResult(entityType, score, start, end);

        Assert.False(first.HasConflict(second));
    }

    [Theory]
    [InlineData(4, 10)]
    [InlineData(4, 9)]
    [InlineData(0, 2)]
    [InlineData(5, 9)]
    public void GreaterThanReturnsTrueWhenFirstPrecedes(int rightStart, int rightEnd)
    {
        var first = CreateResult("ENTITY", 0.2, 5, 10);
        var second = CreateResult("ENTITY", 0.2, rightStart, rightEnd);

        Assert.True(first > second);
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(6, 12)]
    [InlineData(6, 7)]
    public void GreaterThanReturnsFalseWhenFirstDoesNotPrecede(int rightStart, int rightEnd)
    {
        var first = CreateResult("ENTITY", 0.2, 5, 10);
        var second = CreateResult("ENTITY", 0.2, rightStart, rightEnd);

        Assert.False(first > second);
    }

    [Fact]
    public void ConstructorAssignsProperties()
    {
        var result = CreateResult("NUMBER", 0.8, 0, 32);

        Assert.Equal("NUMBER", result.EntityType);
        Assert.Equal(0.8, result.Score);
        Assert.Equal(0, result.Start);
        Assert.Equal(32, result.End);
        Assert.Equal(32, result.Length);
    }

    private static RecognizerResult CreateResult(string entityType, double score, int start, int end)
    {
        var span = new TextSpan(start, end);
        return new RecognizerResult(entityType, span, score);
    }
}
