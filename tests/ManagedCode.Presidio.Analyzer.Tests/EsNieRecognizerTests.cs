using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class EsNieRecognizerTests
{
    [Theory]
    [InlineData("Z8078221M", 0, 9)]
    [InlineData("X9613851N", 0, 9)]
    [InlineData("Y8063915Z", 0, 9)]
    [InlineData("Y8063915-Z", 0, 10)]
    [InlineData("Mi NIE es X9613851N", 10, 19)]
    [InlineData("Z8078221M en mi NIE", 0, 9)]
    [InlineData("Mi Número de identificación de extranjero es Y8063915-Z", 45, 55)]
    public void RecognizerDetectsValidNie(string text, int start, int end)
    {
        var recognizer = new EsNieRecognizer();
        var result = recognizer.Analyze(text, new[] { "ES_NIE" }, new NlpArtifacts("es")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("ES_NIE");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
    }

    [Theory]
    [InlineData("Y8063915Q")]
    [InlineData("Y806391Q")]
    [InlineData("58063915Q")]
    [InlineData("W8063915Q")]
    public void RecognizerFiltersInvalidNie(string text)
    {
        var recognizer = new EsNieRecognizer();
        var results = recognizer.Analyze(text, new[] { "ES_NIE" }, new NlpArtifacts("es"));

        results.ShouldBeEmpty();
    }
}
