using Shouldly;
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

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("CREDIT_CARD");
        match.Score.ShouldBe(EntityRecognizer.MaxScore);
        Slice(text, match).ShouldBe("4916 9944 6504 1084");
    }

    [Theory]
    [InlineData("My credit card number is 4012-8888-8888-1881.", "4012-8888-8888-1881")]
    [InlineData("Card: 6011 0009 9013 9424", "6011 0009 9013 9424")]
    public void AnalyzerNormalizesFormatting(string text, string expectedMatch)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CREDIT_CARD" }).ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("CREDIT_CARD");
        match.Score.ShouldBe(EntityRecognizer.MaxScore);
        Slice(text, match).ShouldBe(expectedMatch);
    }

    [Fact]
    public void AnalyzerFiltersInvalidCreditCard()
    {
        const string text = "My credit card number is 4916 9944 6504 1085.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CREDIT_CARD" }).ToList();

        results.ShouldBeEmpty();
    }

    private static string Slice(string text, RecognizerResult result) =>
        text[result.Start..result.End];
}
