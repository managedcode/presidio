using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UrlRecognizerTests
{
    [Theory]
    [InlineData("https://www.microsoft.com/", "https://www.microsoft.com/", 0.6)]
    [InlineData("http://microsoft.com", "http://microsoft.com", 0.6)]
    [InlineData("http://microsoft.vlaanderen", "http://microsoft.vlaanderen", 0.6)]
    [InlineData("https://webhook.site/a8eedfd6-9d8a-44e0-b0fc-cc7d517db5dc?q=1&b=2", "https://webhook.site/a8eedfd6-9d8a-44e0-b0fc-cc7d517db5dc?q=1&b=2", 0.6)]
    [InlineData("microsoft.com", "microsoft.com", 0.5)]
    [InlineData("\"https://microsoft.github.io/presidio/\"", "\"https://microsoft.github.io/presidio/\"", 0.6)]
    [InlineData("'https://microsoft.github.io/presidio/'", "'https://microsoft.github.io/presidio/'", 0.6)]
    public void RecognizerDetectsUrls(string text, string expectedMatch, double expectedScore)
    {
        var recognizer = new UrlRecognizer();
        var results = recognizer.Analyze(text, new[] { "URL" }, new NlpArtifacts("en")).ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("URL");
        match.Score.ShouldBe(expectedScore, 0.000_01);
        text[match.Start..match.End].ShouldBe(expectedMatch);
    }

    [Fact]
    public void RecognizerExtractsMultipleUrls()
    {
        const string text = "my domains: microsoft.com google.co.il";
        var recognizer = new UrlRecognizer();
        var results = recognizer.Analyze(text, new[] { "URL" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(2);
        text[results[0].Start..results[0].End].ShouldBe("microsoft.com");
        text[results[1].Start..results[1].End].ShouldBe("google.co.il");
    }

    [Theory]
    [InlineData("www.microsoft")]
    [InlineData("http://microsoft")]
    [InlineData("'www.microsoft'")]
    public void RecognizerFiltersInvalidUrls(string text)
    {
        var recognizer = new UrlRecognizer();
        var results = recognizer.Analyze(text, new[] { "URL" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
