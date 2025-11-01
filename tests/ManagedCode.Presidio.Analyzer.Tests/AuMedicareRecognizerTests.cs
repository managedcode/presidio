using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AuMedicareRecognizerTests
{
    [Theory]
    [InlineData("2123 45670 1", "2123 45670 1", 0, 12)]
    [InlineData("2123456701", "2123456701", 0, 10)]
    public void RecognizerDetectsValidMedicare(string text, string expected, int start, int end)
    {
        var recognizer = new AuMedicareRecognizer();
        var result = recognizer.Analyze(text, new[] { "AU_MEDICARE" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("AU_MEDICARE");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
        text[start..end].ShouldBe(expected);
    }

    [Theory]
    [InlineData("2123 25870 1")]
    [InlineData("2123258701")]
    [InlineData("212345670221")]
    [InlineData("2123456702AF")]
    [InlineData("123 456\n789")]
    public void RecognizerFiltersInvalidMedicare(string text)
    {
        var recognizer = new AuMedicareRecognizer();
        var results = recognizer.Analyze(text, new[] { "AU_MEDICARE" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
