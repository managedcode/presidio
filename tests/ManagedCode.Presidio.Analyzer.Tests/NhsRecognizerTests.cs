using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class NhsRecognizerTests
{
    [Theory]
    [InlineData("401-023-2137", "401-023-2137", 0, 12)]
    [InlineData("221 395 1837", "221 395 1837", 0, 12)]
    [InlineData("0032698674", "0032698674", 0, 10)]
    public void RecognizerDetectsValidNhsNumbers(string text, string expectedMatch, int expectedStart, int expectedEnd)
    {
        var recognizer = new NhsRecognizer();
        var results = recognizer.Analyze(text, new[] { "UK_NHS" }, new NlpArtifacts("en")).ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("UK_NHS");
        match.Score.ShouldBe(EntityRecognizer.MaxScore);
        match.Start.ShouldBe(expectedStart);
        match.End.ShouldBe(expectedEnd);
        text[match.Start..match.End].ShouldBe(expectedMatch);
    }

    [Fact]
    public void RecognizerFiltersInvalidNhsNumbers()
    {
        var recognizer = new NhsRecognizer();
        var results = recognizer.Analyze("401-023-2138", new[] { "UK_NHS" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
