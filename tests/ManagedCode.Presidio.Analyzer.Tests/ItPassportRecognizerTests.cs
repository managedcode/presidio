using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ItPassportRecognizerTests
{
    [Theory]
    [InlineData("AA1234567", "AA1234567")]
    [InlineData("aa7654321", "aa7654321")]
    public void RecognizerDetectsItalianPassports(string text, string expected)
    {
        var recognizer = new ItPassportRecognizer();
        var result = recognizer.Analyze(text, new[] { "IT_PASSPORT" }, new NlpArtifacts("it")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("IT_PASSPORT");
        text[result.Start..result.End].ShouldBe(expected);
    }
}
