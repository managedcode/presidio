using System.Reflection;
using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InGstinRecognizerTests
{
    [Theory]
    [InlineData("27ABCDE1234F1Z5", 0, 15)]
    [InlineData("07PQRST6789K1Z2", 0, 15)]
    [InlineData("01ABCDE1234F1Z5", 0, 15)]
    [InlineData("37ABCDE1234F1Z5", 0, 15)]
    [InlineData("My GSTIN number is 27ABCDE1234F1Z5 for business registration", 19, 34)]
    [InlineData("GST registration: 07PQRST6789K1Z2", 18, 33)]
    [InlineData("Tax identification GSTIN: 01ABCDE1234F1Z5", 26, 41)]
    public void RecognizerDetectsValidGstin(string text, int start, int end)
    {
        var recognizer = new InGstinRecognizer();
        var result = recognizer.Analyze(text, new[] { "IN_GSTIN" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("IN_GSTIN");
        result.Score.ShouldBe(EntityRecognizer.MaxScore);
        result.Start.ShouldBe(start);
        result.End.ShouldBe(end);
    }

    [Theory]
    [InlineData("27ABCDE1234F1Z", false)]
    [InlineData("27ABCDE1234F1Z55", false)]
    [InlineData("00ABCDE1234F1Z5", false)]
    [InlineData("38ABCDE1234F1Z5", false)]
    [InlineData("27ABCDE1234F1Y5", false)]
    [InlineData("27ABCDE1234F1Z5", true)]
    public void ValidateResultMatchesPython(string gstin, bool expected)
    {
        var recognizer = new InGstinRecognizer();
        var method = typeof(InGstinRecognizer).GetMethod("ValidateResult", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        var actual = (bool?)method!.Invoke(recognizer, new object[] { gstin });
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("ABCDE1234F", true)]
    [InlineData("PQRST6789K", true)]
    [InlineData("ABCD1234F", false)]
    [InlineData("ABCDE12345F", false)]
    [InlineData("12345ABCDE", false)]
    [InlineData("ABCDE1234", false)]
    public void PanValidationMatchesPython(string pan, bool expected) =>
        InGstinRecognizer.ValidatePanFormat(pan).ShouldBe(expected);

    [Theory]
    [InlineData("27-ABCDE-1234-F1-Z5", "27ABCDE1234F1Z5")]
    [InlineData("27 ABCDE 1234 F1 Z5", "27ABCDE1234F1Z5")]
    [InlineData("The company GSTIN is 27ABCDE1234F1Z5 for tax purposes", "27ABCDE1234F1Z5")]
    public void SanitizerExtractsCanonicalValue(string text, string expected)
    {
        var recognizer = new InGstinRecognizer();
        recognizer.SanitizeValue(text).ShouldBe(expected);
    }

}
