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

        var match = Assert.Single(results);
        Assert.Equal("URL", match.EntityType);
        Assert.Equal(expectedScore, match.Score, 3);
        Assert.Equal(expectedMatch, text[match.Start..match.End]);
    }

    [Fact]
    public void RecognizerExtractsMultipleUrls()
    {
        const string text = "my domains: microsoft.com google.co.il";
        var recognizer = new UrlRecognizer();
        var results = recognizer.Analyze(text, new[] { "URL" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("microsoft.com", text[results[0].Start..results[0].End]);
        Assert.Equal("google.co.il", text[results[1].Start..results[1].End]);
    }

    [Theory]
    [InlineData("www.microsoft")]
    [InlineData("http://microsoft")]
    [InlineData("'www.microsoft'")]
    public void RecognizerFiltersInvalidUrls(string text)
    {
        var recognizer = new UrlRecognizer();
        var results = recognizer.Analyze(text, new[] { "URL" }, new NlpArtifacts("en"));

        Assert.Empty(results);
    }
}
