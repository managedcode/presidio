using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class IbanRecognizerTests
{
    [Theory]
    [InlineData("Bank account: DE89 3704 0044 0532 0130 00", "DE89 3704 0044 0532 0130 00")]
    [InlineData("IBAN: FR14 2004 1010 0505 0001 3M02 606", "FR14 2004 1010 0505 0001 3M02 606")]
    public void AnalyzerDetectsValidIban(string text, string expected)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "IBAN_CODE" }).ToList();

        var match = Assert.Single(results);
        Assert.Equal("IBAN_CODE", match.EntityType);
        Assert.Equal(EntityRecognizer.MaxScore, match.Score);
        Assert.Equal(expected, Slice(text, match));
    }

    [Fact]
    public void AnalyzerFiltersInvalidIbanChecksum()
    {
        const string text = "Account: DE89 3704 0044 0532 0130 01";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "IBAN_CODE" }).ToList();

        Assert.Empty(results);
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
