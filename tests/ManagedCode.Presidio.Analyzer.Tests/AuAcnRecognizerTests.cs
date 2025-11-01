using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AuAcnRecognizerTests
{
    [Theory]
    [InlineData("000 000 019", "000 000 019", 0, 11)]
    [InlineData("005 499 981", "005 499 981", 0, 11)]
    [InlineData("006249976", "006249976", 0, 9)]
    public void RecognizerDetectsValidAcn(string text, string expected, int start, int end)
    {
        var recognizer = new AuAcnRecognizer();
        var result = recognizer.Analyze(text, new[] { "AU_ACN" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("AU_ACN");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
        text[start..end].ShouldBe(expected);
    }

    [Theory]
    [InlineData("824 753 557")]
    [InlineData("824753557")]
    [InlineData("5282475355632")]
    [InlineData("52824753556AF")]
    [InlineData("51 824 753 5564")]
    [InlineData("123 456\n789")]
    public void RecognizerFiltersInvalidAcn(string text)
    {
        var recognizer = new AuAcnRecognizer();
        var results = recognizer.Analyze(text, new[] { "AU_ACN" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
