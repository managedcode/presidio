using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InVoterRecognizerTests
{
    [Theory]
    [InlineData("KSD1287349", "KSD1287349")]
    [InlineData("my voter: DBJ2289013", "DBJ2289013")]
    [InlineData("uzb2345117", "uzb2345117")]
    [InlineData("this MUP5632811", "MUP5632811")]
    [InlineData("You can vote with your CPJ4467918 number", "CPJ4467918")]
    public void RecognizerDetectsValidVoterIds(string text, string expected)
    {
        var recognizer = new InVoterRecognizer();
        var result = recognizer.Analyze(text, new[] { "IN_VOTER" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("IN_VOTER");
        text[result.Start..result.End].ShouldBe(expected);
    }

    [Theory]
    [InlineData("zxdf8923q1")]
    [InlineData("A8923571WZ")]
    public void RecognizerFiltersInvalidVoterIds(string text)
    {
        var recognizer = new InVoterRecognizer();
        var results = recognizer.Analyze(text, new[] { "IN_VOTER" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
