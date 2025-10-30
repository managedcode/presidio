using Xunit;

namespace ManagedCode.Presidio.Core.IntegrationTests;

public sealed class RecognizerResultParityTests
{
    [Theory]
    [MemberData(nameof(ContainsCases))]
    public void ContainsRecognizesNestedSpans(int start, int end)
    {
        var outer = CreateRecognizerResult("", 0, 0, 10);
        var inner = CreateRecognizerResult("", 0, start, end);

        Assert.True(outer.Contains(inner));
    }

    [Theory]
    [MemberData(nameof(DoesNotContainCases))]
    public void ContainsRejectsNonNestedSpans(int start, int end)
    {
        var outer = CreateRecognizerResult("", 0, 5, 10);
        var candidate = CreateRecognizerResult("", 0, start, end);

        Assert.False(outer.Contains(candidate));
    }

    [Fact]
    public void EqualIndicesRecognisesMatchingOffsets()
    {
        var first = CreateRecognizerResult("", 0, 0, 10);
        var second = CreateRecognizerResult("", 0, 0, 10);

        Assert.True(first.EqualIndices(second));
    }

    [Theory]
    [MemberData(nameof(DoesNotContainCases))]
    public void EqualIndicesRejectsDifferentOffsets(int start, int end)
    {
        var first = CreateRecognizerResult("", 0, 5, 10);
        var second = CreateRecognizerResult("", 0, start, end);

        Assert.False(first.EqualIndices(second));
    }

    [Fact]
    public void EqualityConsidersSpanEntityTypeAndScore()
    {
        var first = CreateRecognizerResult("PERSON", 0.2, 0, 10);
        var second = CreateRecognizerResult("PERSON", 0.2, 0, 10);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(InequalityCases))]
    public void InequalityTriggersWhenAnyFieldDiffers(string entityType, double score, int start, int end)
    {
        var first = CreateRecognizerResult("PERSON", 0.2, 0, 10);
        var second = CreateRecognizerResult(entityType, score, start, end);

        Assert.NotEqual(first, second);
        Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(ConflictCases))]
    public void HasConflictMatchesPythonLogic(string entityType, double score, int start, int end)
    {
        var first = CreateRecognizerResult("PERSON", 0.2, 2, 10);
        var second = CreateRecognizerResult(entityType, score, start, end);

        Assert.True(first.HasConflict(second));
    }

    [Theory]
    [MemberData(nameof(NonConflictCases))]
    public void HasConflictReturnsFalseWhenCompatible(string entityType, double score, int start, int end)
    {
        var first = CreateRecognizerResult("PERSON", 0.2, 2, 10);
        var second = CreateRecognizerResult(entityType, score, start, end);

        Assert.False(first.HasConflict(second));
    }

    [Theory]
    [MemberData(nameof(IntersectionCases))]
    public void IntersectsReturnsExpectedLength(int firstStart, int firstEnd, int secondStart, int secondEnd, int expected)
    {
        var first = CreateRecognizerResult("", 0, firstStart, firstEnd);
        var second = CreateRecognizerResult("", 0, secondStart, secondEnd);

        Assert.Equal(expected, first.IntersectionLength(second));
    }

    [Theory]
    [MemberData(nameof(ContainsCases))]
    public void ContainedInRecognisesNestedSpans(int start, int end)
    {
        var outer = CreateRecognizerResult("", 0, 0, 10);
        var inner = CreateRecognizerResult("", 0, start, end);

        Assert.True(inner.IsContainedIn(outer));
    }

    [Theory]
    [MemberData(nameof(GreaterThanCases))]
    public void GreaterThanMatchesPythonOrdering(int firstStart, int firstEnd, int secondStart, int secondEnd, bool isGreater)
    {
        var first = CreateRecognizerResult("", 0, firstStart, firstEnd);
        var second = CreateRecognizerResult("", 0, secondStart, secondEnd);

        Assert.Equal(isGreater, first > second);
    }

    [Fact]
    public void FromDictionaryCreatesResultInstance()
    {
        var payload = new Dictionary<string, object?>
        {
            ["entity_type"] = "NUMBER",
            ["start"] = 0,
            ["end"] = 32,
            ["score"] = 0.8,
        };

        var result = RecognizerResult.FromDictionary(payload);

        Assert.Equal("NUMBER", result.EntityType);
        Assert.Equal(0, result.Start);
        Assert.Equal(32, result.End);
        Assert.Equal(0.8, result.Score, 5);
    }

    public static IEnumerable<object[]> ContainsCases => new[]
    {
        new object[] { 2, 8 },
        new object[] { 0, 8 },
        new object[] { 0, 10 },
    };

    public static IEnumerable<object[]> DoesNotContainCases => new[]
    {
        new object[] { 4, 10 },
        new object[] { 5, 11 },
        new object[] { 0, 5 },
        new object[] { 0, 6 },
    };

    public static IEnumerable<object[]> InequalityCases => new[]
    {
        new object[] { "PERSON", 0.2, 4, 10 },
        new object[] { "ORG", 0.2, 0, 10 },
        new object[] { "PERSON", 0.2, 0, 11 },
        new object[] { "PERSON", 0.3, 0, 10 },
    };

    public static IEnumerable<object[]> ConflictCases => new[]
    {
        new object[] { "PERSON", 0.2, 0, 10 },
        new object[] { "ORG", 0.2, 2, 10 },
        new object[] { "PERSON", 0.3, 0, 11 },
        new object[] { "PERSON", 0.1, 0, 10 },
    };

    public static IEnumerable<object[]> NonConflictCases => new[]
    {
        new object[] { "PERSON", 0.2, 3, 10 },
        new object[] { "ORG", 0.1, 2, 10 },
        new object[] { "PERSON", 0.3, 0, 9 },
    };

    public static IEnumerable<object[]> IntersectionCases => new[]
    {
        new object[] { 0, 10, 4, 12, 6 },
        new object[] { 2, 8, 0, 6, 4 },
        new object[] { 0, 5, 5, 10, 0 },
        new object[] { 3, 7, 0, 3, 0 },
    };

    public static IEnumerable<object[]> GreaterThanCases => new[]
    {
        new object[] { 5, 10, 4, 10, true },
        new object[] { 5, 10, 5, 9, true },
        new object[] { 5, 10, 5, 10, false },
        new object[] { 5, 10, 6, 12, false },
        new object[] { 5, 10, 6, 7, false },
    };

    private static RecognizerResult CreateRecognizerResult(string entityType, double score, int start, int end) =>
        new(entityType, new TextSpan(start, end), score);
}
