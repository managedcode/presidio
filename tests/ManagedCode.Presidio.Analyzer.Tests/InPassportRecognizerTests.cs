using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InPassportRecognizerTests
{
    [Theory]
    [InlineData("passport number: K1234567", "K1234567")]
    [InlineData("My passport is T3569075", "T3569075")]
    [InlineData("Indian passport number Z2345678", "Z2345678")]
    public void RecognizerDetectsValidIndianPassports(string text, string expected)
    {
        var recognizer = new InPassportRecognizer();
        var result = recognizer.Analyze(text, new[] { "IN_PASSPORT" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("IN_PASSPORT");
        text[result.Start..result.End].ShouldBe(expected);
    }

    [Theory]
    [InlineData("passport number: T356907")]
    [InlineData("AA23456789")]
    [InlineData("A1234560")]
    public void RecognizerFiltersInvalidPassport(string text)
    {
        var recognizer = new InPassportRecognizer();
        var results = recognizer.Analyze(text, new[] { "IN_PASSPORT" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
