using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ItFiscalCodeRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("AAAAAA00B11C333Y", 1, new[]{ (0,16) }, new[]{ (0.3, EntityRecognizer.MaxScore) }),
        ("AAAAAA00B11C333N", 1, new[]{ (0,16) }, new[]{ (0.3, EntityRecognizer.MaxScore) }),
        ("AAAAAA - 00B11C333N", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("A55AAA00B11C333N", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("AAAAAA00B11C333Y and AAAAAA00B11C333N", 2, new[]{ (0,16), (21,37) }, new[]{ (0.3, EntityRecognizer.MaxScore), (0.3, EntityRecognizer.MaxScore) }),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new ItFiscalCodeRecognizer();
        var results = recognizer.Analyze(text, new[] { "IT_FISCAL_CODE" }, new NlpArtifacts("it")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedCount);
        for (var i = 0; i < results.Count; i++)
        {
            results[i].Start.ShouldBe(positions[i].Start);
            results[i].End.ShouldBe(positions[i].End);
            results[i].Score.ShouldBeInRange(scoreRanges[i].Min, scoreRanges[i].Max);
        }
    }

    public static IEnumerable<object[]> GetCases()
    {
        foreach (var (text, count, positions, scores) in Cases)
        {
            yield return new object[] { text, count, positions, scores };
        }
    }
}
