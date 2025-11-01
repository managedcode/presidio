using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class MedicalLicenseRecognizerTests
{
    [Theory]
    [InlineData("GL0285191 EU4488929", new[] { "GL0285191", "EU4488929" })]
    [InlineData("K92993548", new[] { "K92993548" })]
    [InlineData("my certificate number is: BB1388568", new[] { "BB1388568" })]
    public void RecognizerDetectsValidMedicalLicenses(string text, string[] expectedMatches)
    {
        var recognizer = new MedicalLicenseRecognizer();
        var results = recognizer.Analyze(text, new[] { "MEDICAL_LICENSE" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedMatches.Length);
        for (var i = 0; i < expectedMatches.Length; i++)
        {
            var result = results[i];
            result.EntityType.ShouldBe("MEDICAL_LICENSE");
            result.Score.ShouldBe(EntityRecognizer.MaxScore);
            text[result.Start..result.End].ShouldBe(expectedMatches[i]);
        }
    }

    [Theory]
    [InlineData("The DEA number is  BG8207031")]
    [InlineData("123 456\n789")]
    public void RecognizerFiltersInvalidMedicalLicenses(string text)
    {
        var recognizer = new MedicalLicenseRecognizer();
        var results = recognizer.Analyze(text, new[] { "MEDICAL_LICENSE" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }
}
