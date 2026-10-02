using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class AuAcnRecognizerTests
{
    [Theory]
    [InlineData("000 000 019", "000 000 019")]
    [InlineData("005 499 981", "005 499 981")]
    [InlineData("006249976", "006249976")]
    public void RecognizerDetectsValidCompanyNumbers(string text, string expected)
    {
        var recognizer = new AuAcnRecognizer();

        var result = recognizer.Analyze(text, ["AU_ACN"], new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("AU_ACN");
        text[result.Start..result.End].ShouldBe(expected);
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
    }

    [Theory]
    [InlineData("824 753 557")]
    [InlineData("824753557")]
    [InlineData("5282475355632")]
    [InlineData("52824753556AF")]
    [InlineData("51 824 753 5564")]
    [InlineData("123 456\n789")]
    public void RecognizerRejectsInvalidCompanyNumbers(string text)
    {
        var recognizer = new AuAcnRecognizer();

        recognizer.Analyze(text, ["AU_ACN"], new NlpArtifacts("en")).ShouldBeEmpty();
    }
}
