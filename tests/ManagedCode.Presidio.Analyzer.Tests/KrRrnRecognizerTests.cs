using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class KrRrnRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("960121-1234567", 1, new[]{ (0,14) }, new[]{ (0.5, 0.5) }),
        ("9601211234567", 1, new[]{ (0,13) }, new[]{ (0.5, 0.5) }),
        ("000505-7637892", 1, new[]{ (0,14) }, new[]{ (0.5, 0.5) }),
        ("0005057637892", 1, new[]{ (0,13) }, new[]{ (0.5, 0.5) }),
        ("His Korean RRN is 960121-1234567", 1, new[]{ (18,32) }, new[]{ (0.5, 0.5) }),
        ("960121-1021413", 1, new[]{ (0,14) }, new[]{ (1.0, 1.0) }),
        ("9601211021413", 1, new[]{ (0,13) }, new[]{ (1.0, 1.0) }),
        ("050912-0000008", 1, new[]{ (0,14) }, new[]{ (1.0, 1.0) }),
        ("0509120000008", 1, new[]{ (0,13) }, new[]{ (1.0, 1.0) }),
        ("His RRN is 9601211021413", 1, new[]{ (11,24) }, new[]{ (1.0, 1.0) }),
        ("001332-1234567", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("0013321234567", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("960121+1021413", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("960121-10214131", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new KrRrnRecognizer();
        var results = recognizer.Analyze(text, new[] { "KR_RRN" }, new NlpArtifacts("ko")).OrderBy(r => r.Start).ToList();

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
