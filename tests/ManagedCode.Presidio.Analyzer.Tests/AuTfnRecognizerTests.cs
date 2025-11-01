using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AuTfnRecognizerTests
{
    [Theory]
    [InlineData("876 543 210", "876 543 210", 0, 11)]
    [InlineData("876543210", "876543210", 0, 9)]
    public void RecognizerDetectsValidTfn(string text, string expected, int start, int end)
    {
        var recognizer = new AuTfnRecognizer();
        var result = recognizer.Analyze(text, new[] { "AU_TFN" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("AU_TFN");
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
    public void RecognizerFiltersInvalidTfn(string text)
    {
        var recognizer = new AuTfnRecognizer();
        var results = recognizer.Analyze(text, new[] { "AU_TFN" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
