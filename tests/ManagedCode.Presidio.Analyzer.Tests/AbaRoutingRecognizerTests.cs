using Shouldly;
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

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("ABA_ROUTING_NUMBER");
        match.Score.ShouldBe(EntityRecognizer.MaxScore);
        Slice(text, match).ShouldBe(expected);
    }

    [Theory]
    [InlineData("Invalid routing: 421042111")]
    [InlineData("Invalid routing: 1234-0000-0")]
    public void AnalyzerFiltersInvalidRoutingNumbers(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "ABA_ROUTING_NUMBER" }).ToList();

        results.ShouldBeEmpty();
    }

    private static string Slice(string text, RecognizerResult result) =>
        text[result.Start..result.End];
}
