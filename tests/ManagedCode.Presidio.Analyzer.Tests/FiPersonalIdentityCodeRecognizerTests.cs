using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class FiPersonalIdentityCodeRecognizerTests
{
    private static readonly string[] ValidCodes =
    {
        "010594Y9032",
        "010594Y9021",
        "020594X903P",
        "020594X902N",
        "030594W903B",
        "030516D9037",
        "020504B904H",
    };

    private static readonly string[] InvalidCodes =
    {
        "111111-111A",
        "111111+110G",
        "311190-1111",
        "310289-211C",
        "012245A110G",
        "010324A110G",
    };

    [Fact]
    public void RecognizerReturnsMaxScoreForValidCodes()
    {
        var recognizer = new FiPersonalIdentityCodeRecognizer();
        foreach (var code in ValidCodes)
        {
            var results = recognizer.Analyze(code, new[] { "FI_PERSONAL_IDENTITY_CODE" }, new NlpArtifacts("fi")).ToList();
            var match = Assert.Single(results);
            Assert.Equal("FI_PERSONAL_IDENTITY_CODE", match.EntityType);
            Assert.Equal(EntityRecognizer.MaxScore, match.Score);
            Assert.Equal(code, code[match.Start..match.End]);
        }
    }

    [Fact]
    public void RecognizerDetectsEmbeddedCodes()
    {
        const string text = "My personal identity code is: 010594Y9032. Thank you.";
        var recognizer = new FiPersonalIdentityCodeRecognizer();

        var results = recognizer.Analyze(text, new[] { "FI_PERSONAL_IDENTITY_CODE" }, new NlpArtifacts("fi")).ToList();

        var match = Assert.Single(results);
        Assert.Equal("010594Y9032", text[match.Start..match.End]);
    }

    [Fact]
    public void RecognizerFiltersInvalidCodes()
    {
        var recognizer = new FiPersonalIdentityCodeRecognizer();
        foreach (var code in InvalidCodes)
        {
            var results = recognizer.Analyze(code, new[] { "FI_PERSONAL_IDENTITY_CODE" }, new NlpArtifacts("fi"));
            Assert.Empty(results);
        }
    }
}
