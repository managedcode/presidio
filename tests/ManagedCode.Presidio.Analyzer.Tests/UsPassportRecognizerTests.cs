using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UsPassportRecognizerTests
{
    [Theory]
    [InlineData("912803456", "912803456", 0.05)]
    [InlineData("Z12803456", "Z12803456", 0.1)]
    [InlineData("A12803456", "A12803456", 0.1)]
    [InlineData("my travel document is A12803456", "A12803456", 0.1)]
    [InlineData("my travel passport is A12803456", "A12803456", 0.1)]
    public void RecognizerDetectsUsPassportNumbers(string text, string expectedMatch, double expectedScore)
    {
        var recognizer = new UsPassportRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_PASSPORT" }, new NlpArtifacts("en"))
            .OrderBy(result => result.Start)
            .ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType);
        match.Score, 5.ShouldBe(expectedScore.ShouldBe("US_PASSPORT");
        text[match.Start..match.End].ShouldBe(expectedMatch);
    }

    [Theory]
    [InlineData("91280345")]
    [InlineData("9128034567")]
    [InlineData("AA2803456")]
    [InlineData("A1280345")]
    public void RecognizerFiltersInvalidPassportNumbers(string text)
    {
        var recognizer = new UsPassportRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_PASSPORT" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}

