using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class PlPeselRecognizerTests
{
    [Theory]
    [InlineData("11111111114", "11111111114")]
    [InlineData("My pesel is 11111111114.", "11111111114")]
    public void RecognizerDetectsValidPesel(string text, string expected)
    {
        var recognizer = new PlPeselRecognizer();
        var results = recognizer.Analyze(text, new[] { "PL_PESEL" }, new NlpArtifacts("pl")).ToList();

        var match = Assert.Single(results);
        Assert.Equal("PL_PESEL", match.EntityType);
        Assert.Equal(EntityRecognizer.MaxScore, match.Score);

        var slice = text[match.Start..match.End];
        Assert.Equal(expected, slice);
    }

    [Theory]
    [InlineData("1111321111")]
    [InlineData("11110021111")]
    [InlineData("11-11-11-11114")]
    public void RecognizerFiltersInvalidPesel(string text)
    {
        var recognizer = new PlPeselRecognizer();
        var results = recognizer.Analyze(text, new[] { "PL_PESEL" }, new NlpArtifacts("pl"));

        Assert.Empty(results);
    }
}
