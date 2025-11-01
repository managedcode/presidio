using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class PhoneRecognizerTests
{
    [Fact]
    public void RecognizerDetectsMultiRegionalNumbers()
    {
        const string text = "My US number is (415) 555-0132, and my international one is +1 415 555 0132";
        var recognizer = new PhoneRecognizer();
        var results = recognizer.Analyze(text, new[] { "PHONE_NUMBER" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        Assert.Equal(2, results.Count);

        Assert.Equal("PHONE_NUMBER", results[0].EntityType);
        Assert.Equal("(415) 555-0132", Slice(text, results[0]));
        Assert.Equal(0.4, results[0].Score, 3);

        Assert.Equal("PHONE_NUMBER", results[1].EntityType);
        Assert.Equal("+1 415 555 0132", Slice(text, results[1]));
        Assert.Equal(0.4, results[1].Score, 3);
    }

    [Theory]
    [InlineData("My US number is (415) 555-0132, and my international one is415-555-0132", 1, 1)]
    [InlineData("My US number is (415) 555-0132, and my international one is415-555-0132", 2, 0)]
    [InlineData("My US number is (415) 555-0132, and my international one is 91-415-555-0132", 1, 2)]
    [InlineData("My US number is (415) 555-0132, and my international one is 91-415-555-0132", 2, 1)]
    [InlineData("My US number is (415) 555-0132, and my international one is +91 4155 550132", 1, 3)]
    [InlineData("My US number is (415) 555-0132, and my international one is +91 4155 550132", 2, 2)]
    [InlineData("My US number is (415) 555-0132, and my international one is +91 4155550132", 2, 3)]
    public void RecognizerHonorsLeniencySetting(string text, int expectedCount, int leniency)
    {
        var recognizer = new PhoneRecognizer(leniency: leniency);
        var results = recognizer.Analyze(text, new[] { "PHONE_NUMBER" }, new NlpArtifacts("en"));

        Assert.Equal(expectedCount, results.Count);
    }

    [Theory]
    [InlineData("My US number is (415) 555-0132, and my international one is +44 (20) 7123 4567", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as GB region phone number, using PhoneRecognizer" })]
    [InlineData("My US number is (415) 555-0132, and my international one is +91 4155550132", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as IN region phone number, using PhoneRecognizer" })]
    [InlineData("My US number is (415) 555-0132, and my international one is +55 11 98456 5666", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as BR region phone number, using PhoneRecognizer" })]
    [InlineData("My US number is (415) 555-0132, and my international one is +49 30 1234567", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as DE region phone number, using PhoneRecognizer" })]
    [InlineData("My US number is (415) 555-0132, and my international one is +39 06 678 4343", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as IT region phone number, using PhoneRecognizer" })]
    [InlineData("My US number is (415) 555-0132, and my international one is +30 21 0 1234567", new[] { "Recognized as US region phone number, using PhoneRecognizer", "Recognized as GR region phone number, using PhoneRecognizer" })]
    public void RecognizerProvidesRegionSpecificExplanation(string text, string[] expectedExplanations)
    {
        var recognizer = new PhoneRecognizer();
        var results = recognizer.Analyze(text, new[] { "PHONE_NUMBER" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        Assert.Equal(expectedExplanations.Length, results.Count);
        for (var i = 0; i < expectedExplanations.Length; i++)
        {
            var explanation = results[i].AnalysisExplanation;
            Assert.NotNull(explanation);
            Assert.Equal(expectedExplanations[i], explanation!.TextualExplanation);
            Assert.Equal(PhoneRecognizer.DefaultScore, explanation.Score, 3);
        }
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
