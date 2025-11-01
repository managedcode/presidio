using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ItVatCodeRecognizerTests
{
    [Theory]
    [InlineData("00000000000", 0)]
    [InlineData("00000000001", 0)]
    [InlineData("01333550323", 1)]
    [InlineData("00000000000 and 01333550323", 1)]
    [InlineData("01333550_323", 1)]
    public void RecognizerDetectsVatCodes(string text, int expectedCount)
    {
        var recognizer = new ItVatCodeRecognizer();
        var results = recognizer.Analyze(text, new[] { "IT_VAT_CODE" }, new NlpArtifacts("it")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedCount);
        if (results.Count > 0)
        {
            results[0].Score.ShouldBeGreaterThanOrEqualTo(0.9);
        }
    }
}
