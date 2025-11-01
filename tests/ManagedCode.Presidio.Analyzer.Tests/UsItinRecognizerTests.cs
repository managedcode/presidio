using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UsItinRecognizerTests
{
    [Theory]
    [InlineData("911-701234 91170-1234", new[] { "911-701234", "91170-1234" }, new[] { 0.05, 0.05 })]
    [InlineData("911701234", new[] { "911701234" }, new[] { 0.3 })]
    [InlineData("911-70-1234", new[] { "911-70-1234" }, new[] { 0.5 })]
    [InlineData("911-53-1234", new[] { "911-53-1234" }, new[] { 0.5 })]
    [InlineData("911-64-1234", new[] { "911-64-1234" }, new[] { 0.5 })]
    public void RecognizerDetectsValidItins(string text, string[] expectedMatches, double[] expectedScores)
    {
        var recognizer = new UsItinRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_ITIN" }, new NlpArtifacts("en"))
            .OrderBy(result => result.Start)
            .ToList();

        results.Count);

        for (var i = 0; i < expectedMatches.Length; i++)
        {
            var match = results[i];
            match.EntityType.ShouldBe("US_ITIN".ShouldBe(expectedMatches.Length);
            match.Score.ShouldBe(expectedScores[i], 5);
            text[match.Start..match.End].ShouldBe(expectedMatches[i]);
        }
    }

    [Theory]
    [InlineData("911-89-1234")]
    [InlineData("my tax id 911-89-1234")]
    public void RecognizerFiltersInvalidItins(string text)
    {
        var recognizer = new UsItinRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_ITIN" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}

