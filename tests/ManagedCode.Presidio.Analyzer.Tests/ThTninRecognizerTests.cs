using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class ThTninRecognizerTests
{
    private static readonly (string Text, int ExpectedCount, (int Start, int End)[] Positions, (double Min, double Max)[] ScoreRanges)[] Cases =
    {
        ("1234567890121", 1, new[]{ (0,13) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("2345678901234", 1, new[]{ (0,13) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("3456789012347", 1, new[]{ (0,13) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("4567890123459", 1, new[]{ (0,13) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("5678901234560", 1, new[]{ (0,13) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("My Thai ID is 1234567890121", 1, new[]{ (14,27) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("TNIN: 2345678901234", 1, new[]{ (6,19) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("เลขประจำตัวประชาชน: 3456789012347", 1, new[]{ (20,33) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("123456789012", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("12345678901234", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("123456789012a", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("123456789012 ", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("0234567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("0034567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1034567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1304567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1284567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1294567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1594567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1684567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1694567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1784567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1794567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1874567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1884567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1894567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1974567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1984567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1994567890124", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1234567890123", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("2345678901235", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("3456789012346", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("0000000000000", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("1111111111111", 0, Array.Empty<(int,int)>(), Array.Empty<(double,double)>()),
        ("Thai National ID 1234567890121", 1, new[]{ (17,30) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
        ("เลขบัตรประชาชน 2345678901234", 1, new[]{ (15,28) }, new[]{ (0.5, EntityRecognizer.MaxScore) }),
    };

    [Theory]
    [MemberData(nameof(GetCases))]
    public void RecognizerMatchesPythonExpectations(string text, int expectedCount, (int Start, int End)[] positions, (double Min, double Max)[] scoreRanges)
    {
        var recognizer = new ThTninRecognizer();
        var results = recognizer.Analyze(text, new[] { "TH_TNIN" }, new NlpArtifacts("th")).OrderBy(r => r.Start).ToList();

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
