using Xunit;

namespace ManagedCode.Presidio.Core.Tests;

public sealed class TextSpanTests
{
    [Fact]
    public void ContainsReturnsTrueWhenIndexInside()
    {
        var span = new TextSpan(2, 8);

        Assert.True(span.Contains(2));
        Assert.True(span.Contains(7));
        Assert.False(span.Contains(1));
        Assert.False(span.Contains(8));
    }

    [Fact]
    public void UnionProducesEnclosingSpan()
    {
        var first = new TextSpan(0, 4);
        var second = new TextSpan(2, 6);

        var union = first.Union(second);

        Assert.Equal(0, union.Start);
        Assert.Equal(6, union.End);
    }

    [Theory]
    [InlineData(0, 4, 4, 8, false)]
    [InlineData(0, 5, 5, 9, false)]
    [InlineData(1, 6, 2, 5, true)]
    [InlineData(1, 6, 1, 6, true)]
    public void IntersectsMatchesExpectedResult(int firstStart, int firstEnd, int secondStart, int secondEnd, bool expected)
    {
        var first = new TextSpan(firstStart, firstEnd);
        var second = new TextSpan(secondStart, secondEnd);

        Assert.Equal(expected, first.Intersects(second));
    }
}
