using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class EsNifRecognizerTests
{
    [Theory]
    [InlineData("55555555K", 0, 9)]
    [InlineData("55555555-K", 0, 10)]
    [InlineData("1111111-G", 0, 9)]
    [InlineData("1111111G", 0, 8)]
    [InlineData("01111111G", 0, 9)]
    public void RecognizerDetectsValidNif(string text, int start, int end)
    {
        var recognizer = new EsNifRecognizer();
        var result = recognizer.Analyze(text, new[] { "ES_NIF" }, new NlpArtifacts("es")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("ES_NIF");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
    }

    [Theory]
    [InlineData("401-023-2138")]
    public void RecognizerFiltersInvalidNif(string text)
    {
        var recognizer = new EsNifRecognizer();
        var results = recognizer.Analyze(text, new[] { "ES_NIF" }, new NlpArtifacts("es"));

        results.ShouldBeEmpty();
    }
}
