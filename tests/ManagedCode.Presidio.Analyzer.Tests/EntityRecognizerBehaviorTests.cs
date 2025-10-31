using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class EntityRecognizerBehaviorTests
{
    [Fact]
    public void RemoveDuplicatesPrefersHigherScore()
    {
        var lowScore = CreateResult("X", 0.1, 0, 5);
        var highScore = CreateResult("X", 0.9, 0, 5);

        var results = EntityRecognizer.RemoveDuplicates(new[] { lowScore, highScore });

        var single = Assert.Single(results);
        Assert.Equal(0.9, single.Score);
    }

    [Fact]
    public void RemoveDuplicatesKeepsDifferentEntities()
    {
        var first = CreateResult("X", 0.5, 0, 5);
        var second = CreateResult("Y", 0.6, 0, 5);

        var results = EntityRecognizer.RemoveDuplicates(new[] { first, second });

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void RemoveDuplicatesPrefersOuterSpanWhenScoresEqual()
    {
        var outer = CreateResult("X", 0.5, 0, 10);
        var inner = CreateResult("X", 0.5, 0, 5);

        var results = EntityRecognizer.RemoveDuplicates(new[] { outer, inner });

        var single = Assert.Single(results);
        Assert.Same(outer, single);
    }

    [Theory]
    [MemberData(nameof(SanitizeData))]
    public void SanitizeValueRemovesTokens(string input, IEnumerable<(string Search, string Replacement)> replacements, string expected)
    {
        var output = EntityRecognizer.SanitizeValue(input, replacements);
        Assert.Equal(expected, output);
    }

    private static RecognizerResult CreateResult(string entity, double score, int start, int end)
    {
        var explanation = new AnalysisExplanation("test", 0, patternName: "test", pattern: "test");
        return new RecognizerResult(entity, new TextSpan(start, end), score, explanation);
    }

    public static IEnumerable<object[]> SanitizeData()
    {
        yield return new object[]
        {
            "  a|b:c       ::-",
            new (string Search, string Replacement)[]
            {
                ("-", string.Empty),
                (" ", string.Empty),
                (":", string.Empty),
                ("|", string.Empty),
            },
            "abc"
        };

        yield return new object[]
        {
            "def",
            Array.Empty<(string Search, string Replacement)>(),
            "def"
        };
    }
}
