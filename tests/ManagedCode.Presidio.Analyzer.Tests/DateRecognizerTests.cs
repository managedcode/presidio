using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class DateRecognizerTests
{
    public static IEnumerable<object[]> ValidDateCases => new[]
    {
        new object[] { "Today is 5-20-2021", "5-20-2021", 0.6 },
        new object[] { "Today is 5/20/2021", "5/20/2021", 0.6 },
        new object[] { "Today is 2021-05-21", "2021-05-21", 0.6 },
        new object[] { "Today is 21.5.2021", "21.5.2021", 0.6 },
        new object[] { "Today is 5-MAY-2021", "5-MAY-2021", 0.6 },
        new object[] { "Today is May-2021", "May-2021", 0.6 },
        new object[] { "Today is 05/21/21", "05/21/21", 0.6 },
        new object[] { "Today is May-21", "May-21", 0.6 },
        new object[] { "Today is 05/2021", "05/2021", 0.2 },
        new object[] { "Today is 05/21", "05/21", 0.1 },
        new object[] { "Today is 2024-06-05T09:15:30.500-07:00 or not?", "2024-06-05T09:15:30.500-07:00", 0.8 },
        new object[] { "Today is 2024-03-15T14:30:00Z, or not?", "2024-03-15T14:30:00Z", 0.8 },
    };

    [Theory]
    [MemberData(nameof(ValidDateCases))]
    public void AnalyzerDetectsValidDates(string text, string expected, double expectedScore)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "DATE_TIME" }).ToList();

        var match = results.FirstOrDefault(r =>
            r.EntityType == "DATE_TIME" &&
            Slice(text, r).Equals(expected, StringComparison.Ordinal));

        match.ShouldNotBeNull();
        match!.Score.ShouldBe(expectedScore, 0.000_01);
    }

    [Theory]
    [InlineData("Today is2024-06-05T09:15:30.500-07:00")]
    [InlineData("Today is5/21")]
    [InlineData("Today is5/21and it's sunny")]
    [InlineData("Tomorrow maybe?")]
    public void AnalyzerFiltersInvalidDates(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "DATE_TIME" }).ToList();

        results.ShouldBeEmpty();
    }

    [Fact]
    public void RecognizerEmitsExpectedScore()
    {
        var recognizer = new DateRecognizer();
        var results = recognizer.Analyze("2024-03-15T14:30Z", new[] { "DATE_TIME" }, new NlpArtifacts("en"));
        var match = results.ShouldHaveSingleItem();
        match.EntityType.ShouldBe("DATE_TIME");
        match.Score.ShouldBe(0.8, 0.000_01);
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
