using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InPanRecognizerTests
{
    [Theory]
    [InlineData("ABCDE1234F", true)]
    [InlineData("AAAPL1234C", true)]
    [InlineData("pan: AAAPL1234C", true)]
    [InlineData("AAAPL1234", false)]
    [InlineData("1234512345", false)]
    public void RecognizerValidatesPanNumbers(string text, bool expected)
    {
        var recognizer = new InPanRecognizer();
        var results = recognizer.Analyze(text, new[] { "IN_PAN" }, new NlpArtifacts("en")).ToList();

        if (expected)
        {
            results.ShouldNotBeEmpty();
        }
        else
        {
            results.ShouldBeEmpty();
        }
    }
}
