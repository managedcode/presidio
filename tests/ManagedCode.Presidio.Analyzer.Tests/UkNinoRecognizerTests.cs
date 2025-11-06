using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class UkNinoRecognizerTests
{
    [Theory]
    [InlineData("AA 12 34 56 B", "AA 12 34 56 B", 0, 13)]
    [InlineData("hh 01 02 03 d", "hh 01 02 03 d", 0, 13)]
    [InlineData("tw987654a", "tw987654a", 0, 9)]
    [InlineData("nino: PR 123612C", "PR 123612C", 6, 16)]
    [InlineData("Here is my National Insurance Number YZ 61 48 68 B", "YZ 61 48 68 B", 36, 50)]
    public void RecognizerDetectsValidUkNinos(string text, string expectedMatch, int expectedStart, int expectedEnd)
    {
        var recognizer = new UkNinoRecognizer();
        var results = recognizer.Analyze(text, new[] { "UK_NINO" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("UK_NINO");
        match.Score.ShouldBe(EntityRecognizer.MaxScore);
        match.Start.ShouldBe(expectedStart);
        match.End.ShouldBe(expectedEnd);
        text[match.Start..match.End].ShouldBe(expectedMatch);
    }

    [Theory]
    [InlineData("AA 12 34 56 H")]
    [InlineData("FQ 00 00 00 C")]
    [InlineData("BG123612A")]
    [InlineData("nino: nt 99 88 77 a")]
    [InlineData("This isn't a valid national insurance number UV 98 76 54 B")]
    public void RecognizerFiltersInvalidUkNinos(string text)
    {
        var recognizer = new UkNinoRecognizer();
        var results = recognizer.Analyze(text, new[] { "UK_NINO" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
