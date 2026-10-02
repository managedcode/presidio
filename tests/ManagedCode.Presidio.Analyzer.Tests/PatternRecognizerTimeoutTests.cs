using System.Text.RegularExpressions;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class PatternRecognizerTimeoutTests
{
    [Fact]
    public void Analyze_PropagatesRegexMatchTimeoutInsteadOfReturningNoFindings()
    {
        var recognizer = new PatternRecognizer(
            "CUSTOM_ID",
            [new Pattern("pathological", "^(a+)+$", 0.9)],
            regexMatchTimeout: TimeSpan.FromSeconds(10));
        var registry = new RecognizerRegistry([recognizer], ["en"], TimeSpan.FromMilliseconds(10));
        using var analyzer = new AnalyzerEngine(registry);

        var text = new string('a', 10_000) + "!";

        Assert.Throws<RegexMatchTimeoutException>(() => analyzer.Analyze(text, "en", ["CUSTOM_ID"]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PatternRecognizer_RejectsNonPositiveRegexMatchTimeout(int milliseconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PatternRecognizer(
            "CUSTOM_ID",
            [new Pattern("pathological", "^a+$", 0.9)],
            regexMatchTimeout: TimeSpan.FromMilliseconds(milliseconds)));
    }
}
