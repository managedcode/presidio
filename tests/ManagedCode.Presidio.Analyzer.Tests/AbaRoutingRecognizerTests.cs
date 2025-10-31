using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AbaRoutingRecognizerTests
{
    [Theory]
    [InlineData("Routing number: 121000358", "121000358")]
    [InlineData("Routing number: 3222-7162-7", "3222-7162-7")]
    public void AnalyzerDetectsValidRoutingNumbers(string text, string expected)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "ABA_ROUTING_NUMBER" }).ToList();

        var match = Assert.Single(results);
        Assert.Equal("ABA_ROUTING_NUMBER", match.EntityType);
        Assert.Equal(EntityRecognizer.MaxScore, match.Score);
        Assert.Equal(expected, Slice(text, match));
    }

    [Theory]
    [InlineData("Invalid routing: 421042111")]
    [InlineData("Invalid routing: 1234-0000-0")]
    public void AnalyzerFiltersInvalidRoutingNumbers(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "ABA_ROUTING_NUMBER" }).ToList();

        Assert.Empty(results);
    }

    private static string Slice(string text, RecognizerResult result) =>
        text[result.Start..result.End];
}
