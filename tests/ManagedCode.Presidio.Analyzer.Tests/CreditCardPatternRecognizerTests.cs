using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class CreditCardPatternRecognizerTests
{
    [Fact]
    public void AnalyzerDetectsValidCreditCard()
    {
        const string text = "My credit card number is 4916 9944 6504 1084.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CREDIT_CARD" }).ToList();

        var match = Assert.Single(results);
        Assert.Equal("CREDIT_CARD", match.EntityType);
        Assert.Equal(EntityRecognizer.MaxScore, match.Score);
        Assert.Equal("4916 9944 6504 1084", Slice(text, match));
    }

    [Theory]
    [InlineData("My credit card number is 4012-8888-8888-1881.")]
    [InlineData("Card: 6011 0009 9013 9424")]
    public void AnalyzerNormalizesFormatting(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CREDIT_CARD" }).ToList();

        Assert.Single(results);
    }

    [Fact]
    public void AnalyzerFiltersInvalidCreditCard()
    {
        const string text = "My credit card number is 4916 9944 6504 1085.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CREDIT_CARD" }).ToList();

        Assert.Empty(results);
    }

    private static string Slice(string text, RecognizerResult result) =>
        text[result.Start..result.End];
}
