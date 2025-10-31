using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class EmailRecognizerTests
{
    [Theory]
    [InlineData("info@presidio.site", "info@presidio.site")]
    [InlineData("Contact us via info@presidio.site", "info@presidio.site")]
    [InlineData("Try info@presidio.site or anotherinfo@presidio.site", "info@presidio.site")]
    [InlineData("Try info@presidio.site or anotherinfo@presidio.site", "anotherinfo@presidio.site")]
    [InlineData("ユーザー test@例え.テスト", "test@例え.テスト")]
    [InlineData("Punycode xn--user-jta@xn--d1acpjx3f.xn--p1ai", "xn--user-jta@xn--d1acpjx3f.xn--p1ai")]
    public void AnalyzerDetectsValidEmails(string text, string expected)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "EMAIL_ADDRESS" }).ToList();

        Assert.Contains(results, r =>
        {
            if (!string.Equals(r.EntityType, "EMAIL_ADDRESS", StringComparison.Ordinal))
            {
                return false;
            }

            var segment = Slice(text, r);
            return string.Equals(segment, expected, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData("my email is info@presidio.")]
    [InlineData("invalid@domain")]
    [InlineData("missing@tld.")]
    [InlineData("double@@example.com")]
    [InlineData("no-at-symbol.example.com")]
    public void AnalyzerFiltersInvalidEmails(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "EMAIL_ADDRESS" }).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void RecognizerProducesMaxScore()
    {
        var recognizer = new EmailRecognizer();
        var results = recognizer.Analyze("info@presidio.site", new[] { "EMAIL_ADDRESS" }, new NlpArtifacts("en"));

        var match = Assert.Single(results);
        Assert.Equal(EntityRecognizer.MaxScore, match.Score);
        Assert.Equal("EMAIL_ADDRESS", match.EntityType);
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
