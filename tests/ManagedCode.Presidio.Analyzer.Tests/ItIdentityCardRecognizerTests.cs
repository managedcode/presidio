using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ItIdentityCardRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("AA 1234567", 1, new[]{ (0,10) }, new[]{ (0.0, 0.05) }),
        ("AA 1234567 and AA 1234567", 2, new[]{ (0,10), (15,25) }, new[]{ (0.0, 0.05), (0.0, 0.05) }),
        ("My identity number is AA 1234567", 1, new[]{ (22,32) }, new[]{ (0.0, 0.05) }),
        ("AA123456", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1234567Aa", 1, new[]{ (0,9) }, new[]{ (0.0, 0.05) }),
        ("AA12345aa", 1, new[]{ (0,9) }, new[]{ (0.0, 0.05) }),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new ItIdentityCardRecognizer();
        var results = recognizer.Analyze(text, new[] { "IT_IDENTITY_CARD" }, new NlpArtifacts("it")).OrderBy(r => r.Start).ToList();

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
