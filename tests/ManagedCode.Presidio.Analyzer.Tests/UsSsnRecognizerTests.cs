using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UsSsnRecognizerTests
{
    [Theory]
    [InlineData("078-051121 07805-1121", new[] { "078-051121", "07805-1121" }, 0.05)]
    [InlineData("078051121", new[] { "078051121" }, 0.05)]
    [InlineData("078-05-1123", new[] { "078-05-1123" }, 0.5)]
    [InlineData("078.05.1123", new[] { "078.05.1123" }, 0.5)]
    [InlineData("078 05 1123", new[] { "078 05 1123" }, 0.5)]
    [InlineData("abc 078 05 1123 abc", new[] { "078 05 1123" }, 0.5)]
    public void RecognizerDetectsValidSsns(string text, string[] expectedMatches, double minimumScore)
    {
        var recognizer = new UsSsnRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_SSN" }, new NlpArtifacts("en"))
            .OrderBy(r => r.Start)
            .ToList();

        results.Count.ShouldBe(expectedMatches.Length);

        for (var i = 0; i < expectedMatches.Length; i++)
        {
            var match = results[i];
            match.EntityType.ShouldBe("US_SSN");
            match.Score.ShouldBeGreaterThanOrEqualTo(minimumScore);
            text[match.Start..match.End].ShouldBe(expectedMatches[i]);
        }
    }

    [Theory]
    [InlineData("0780511201")]
    [InlineData("078051120")]
    [InlineData("000000000")]
    [InlineData("666000000")]
    [InlineData("078-05-0000")]
    [InlineData("078 00 1123")]
    [InlineData("693-09.4444")]
    public void RecognizerFiltersInvalidSsns(string text)
    {
        var recognizer = new UsSsnRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_SSN" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
