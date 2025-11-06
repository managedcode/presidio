using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class TextReplaceBuilderTests
{
    [Theory]
    [InlineData("hello world", 0, 5, "", " world", 6)]
    [InlineData("hello world", 5, 5, "bla", "hellobla world", 9)]
    [InlineData("hello world", 5, 12, "bla", "hellobla", 3)]
    [InlineData("The url is http://microsofy.com", 11, 31, "", "The url is ", 0)]
    public void ReplaceTextUpdatesOutputAndReturnsInsertionOffset(
        string original,
        int start,
        int end,
        string replacement,
        string expectedOutput,
        int expectedIndex)
    {
        var builder = new TextReplaceBuilder(original);

        var index = builder.ReplaceTextGetInsertionIndex(replacement, start, end);

        Assert.Equal(expectedOutput, builder.OutputText);
        Assert.Equal(expectedIndex, index);
    }

    [Theory]
    [InlineData("hello world", 0, 5, "hello")]
    [InlineData("hello world", 5, 5, "")]
    [InlineData("hello world", 6, 11, "world")]
    [InlineData("hello world", 0, 0, "")]
    public void GetTextInPositionReturnsOriginalSlice(string original, int start, int end, string expected)
    {
        var builder = new TextReplaceBuilder(original);

        var slice = builder.GetTextInPosition(start, end);

        Assert.Equal(expected, slice);
    }

    [Theory]
    [InlineData("hello world", 0, 15)]
    [InlineData("hello world", 12, 5)]
    [InlineData("hello world", 15, 16)]
    public void InvalidBoundsThrowInvalidParamException(string original, int start, int end)
    {
        var builder = new TextReplaceBuilder(original);

        var exception = Assert.Throws<InvalidParamException>(() => builder.GetTextInPosition(start, end));
        Assert.Equal(
            $"Invalid analyzer result, start: {start} and end: {end}, while text length is only {original.Length}.",
            exception.Message);
    }
}
