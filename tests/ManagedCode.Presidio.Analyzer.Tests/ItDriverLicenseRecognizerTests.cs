using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ItDriverLicenseRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("AA0123456B", 1, new[]{ (0,10) }, new[]{ (0.1, 0.4) }),
        ("AA0123456B and AA0123456B", 2, new[]{ (0,10), (15,25) }, new[]{ (0.1,0.4), (0.1,0.4) }),
        ("U1H00B000C", 1, new[]{ (0,10) }, new[]{ (0.1,0.4) }),
        ("U1H00A000B", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("990123456B", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new ItDriverLicenseRecognizer();
        var results = recognizer.Analyze(text, new[] { "IT_DRIVER_LICENSE" }, new NlpArtifacts("it")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedCount);
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            result.Start.ShouldBe(positions[i].Start);
            result.End.ShouldBe(positions[i].End);
            result.Score.ShouldBeInRange(scoreRanges[i].Min, scoreRanges[i].Max);
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
