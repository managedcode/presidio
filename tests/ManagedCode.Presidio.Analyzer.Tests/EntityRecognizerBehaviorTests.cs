using Shouldly;
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

        var single = results.ShouldHaveSingleItem();
        single.Score.ShouldBe(0.9);
    }

    [Fact]
    public void RemoveDuplicatesKeepsDifferentEntities()
    {
        var first = CreateResult("X", 0.5, 0, 5);
        var second = CreateResult("Y", 0.6, 0, 5);

        var results = EntityRecognizer.RemoveDuplicates(new[] { first, second });

        results.Count.ShouldBe(2);
    }

    [Fact]
    public void RemoveDuplicatesPrefersOuterSpanWhenScoresEqual()
    {
        var outer = CreateResult("X", 0.5, 0, 10);
        var inner = CreateResult("X", 0.5, 0, 5);

        var results = EntityRecognizer.RemoveDuplicates(new[] { outer, inner });

        var single = results.ShouldHaveSingleItem();
        single.ShouldBeSameAs(outer);
    }

    [Theory]
    [MemberData(nameof(SanitizeData))]
    public void SanitizeValueRemovesTokens(string input, IEnumerable<(string Search, string Replacement)> replacements, string expected)
    {
        var output = EntityRecognizer.SanitizeValue(input, replacements);
        output.ShouldBe(expected);
    }

    [Fact]
    public void ToDictionaryReflectsRecognizerConfiguration()
    {
        var recognizer = new TestRecognizer();

        var dictionary = recognizer.ToDictionary();

        dictionary[nameof(EntityRecognizer.SupportedEntities)].ShouldBe(new[] { "ENTITY" });
        dictionary[nameof(EntityRecognizer.SupportedLanguage)].ShouldBe("en");
        dictionary[nameof(EntityRecognizer.Name)].ShouldBe("TestRecognizer");
        dictionary[nameof(EntityRecognizer.Version)].ShouldBe("0.0.1");
    }

    private sealed class TestRecognizer : EntityRecognizer
    {
        public TestRecognizer()
            : base(new[] { "ENTITY" })
        {
        }

        protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
            string text,
            IReadOnlyCollection<string> entities,
            NlpArtifacts artifacts)
        {
            return Array.Empty<RecognizerResult>();
        }
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
