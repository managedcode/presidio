using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AuAbnRecognizerTests
{
    [Theory]
    [InlineData("51 824 753 556", "51 824 753 556", 0, 14)]
    [InlineData("51824753556", "51824753556", 0, 11)]
    public void RecognizerDetectsValidAbn(string text, string expected, int start, int end)
    {
        var recognizer = new AuAbnRecognizer();
        var result = recognizer.Analyze(text, new[] { "AU_ABN" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("AU_ABN");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
        text[start..end].ShouldBe(expected);
    }

    [Theory]
    [InlineData("52 824 753 556")]
    [InlineData("52824753556")]
    [InlineData("5282475355632")]
    [InlineData("52824753556AF")]
    [InlineData("51 824 753 5564")]
    [InlineData("123 456\n789")]
    public void RecognizerFiltersInvalidAbn(string text)
    {
        var recognizer = new AuAbnRecognizer();
        var results = recognizer.Analyze(text, new[] { "AU_ABN" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
