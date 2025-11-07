using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class SgUenRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions)[] Cases =
    {
        ("53125226D", 1, new[]{ (0,9) }),
        ("201434292D", 1, new[]{ (0,10) }),
        ("T16RF0037C", 1, new[]{ (0,10) }),
        ("S57TU0392K", 1, new[]{ (0,10) }),
        ("53125226D 201434292D S57TU0392K", 3, new[]{ (0,9), (10,20), (21,31) }),
        ("UEN 53125226D was processed", 1, new[]{ (4,13) }),
        ("53125226", 0, Array.Empty<(int,int)>()),
        ("", 0, Array.Empty<(int,int)>()),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions)
    {
        var recognizer = new SgUenRecognizer();
        var results = recognizer.Analyze(text, new[] { "SG_UEN" }, new NlpArtifacts("en")).OrderBy(r => r.Start).ToList();

        results.Count.ShouldBe(expectedCount);
        for (var i = 0; i < results.Count; i++)
        {
            results[i].Start.ShouldBe(positions[i].Start);
            results[i].End.ShouldBe(positions[i].End);
            results[i].Score.ShouldBe(EntityRecognizer.MaxScore);
        }
    }

    public static IEnumerable<object[]> GetCases()
    {
        foreach (var (text, count, positions) in Cases)
        {
            yield return new object[] { text, count, positions };
        }
    }
}
