using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UsBankRecognizerTests
{
    [Theory]
    [InlineData("945456787654", "945456787654", 0, 12)]
    public void RecognizerDetectsValidBankAccounts(string text, string expectedMatch, int expectedStart, int expectedEnd)
    {
        var recognizer = new UsBankRecognizer();
        var results = recognizer.Analyze(text, new[] { "US_BANK_NUMBER" }, new NlpArtifacts("en")).ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("US_BANK_NUMBER");
        match.Score.ShouldBe(0.05, 5);
        match.Start.ShouldBe(expectedStart);
        match.End.ShouldBe(expectedEnd);
        text[match.Start..match.End].ShouldBe(expectedMatch);
    }

    [Fact]
    public void RecognizerFiltersInvalidBankAccounts()
    {
        var recognizer = new UsBankRecognizer();
        var results = recognizer.Analyze("1234567", new[] { "US_BANK_NUMBER" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
