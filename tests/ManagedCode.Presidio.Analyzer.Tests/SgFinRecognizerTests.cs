using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class SgFinRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("S2740116C", 1, new[]{ (0,9) }, new[]{ (0.5, 0.8) }),
        ("T1234567Z", 1, new[]{ (0,9) }, new[]{ (0.5, 0.8) }),
        ("F2346401L", 1, new[]{ (0,9) }, new[]{ (0.5, 0.8) }),
        ("G1122144L", 1, new[]{ (0,9) }, new[]{ (0.5, 0.8) }),
        ("M4332674T", 1, new[]{ (0,9) }, new[]{ (0.5, 0.8) }),
        ("S9108268C T7572225C", 2, new[]{ (0,9), (10,19) }, new[]{ (0.5, 0.8), (0.5, 0.8) }),
        ("NRIC S2740116C was processed", 1, new[]{ (5,14) }, new[]{ (0.5, 0.8) }),
        ("A1234567Z", 1, new[]{ (0,9) }, new[]{ (0.0, 0.3) }),
        ("B1234567Z", 1, new[]{ (0,9) }, new[]{ (0.0, 0.3) }),
        ("PA12348L", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new SgFinRecognizer();
        var results = recognizer.Analyze(text, new[] { "SG_NRIC_FIN" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedCount);
        for (var i = 0; i < results.Count; i++)
        {
            results[i].Start.ShouldBe(positions[i].Start);
            results[i].End.ShouldBe(positions[i].End);
            results[i].Score.ShouldBeInRange(scoreRanges[i].Min, scoreRanges[i].Max);
        }
    }

    public static TheoryData<string, int, (int, int)[], (double, double)[]> GetCases()
    {
        var data = new TheoryData<string, int, (int, int)[], (double, double)[]>();
        foreach (var (text, count, positions, scores) in Cases)
        {
            data.Add(text, count, positions, scores);
        }

        return data;
    }
}
