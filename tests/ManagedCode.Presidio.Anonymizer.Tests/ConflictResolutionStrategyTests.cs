using ManagedCode.Presidio.Core;
using Xunit;
using CoreRecognizerResult = ManagedCode.Presidio.Core.RecognizerResult;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class ConflictResolutionStrategyTests
{
    public static IEnumerable<object?[]> MergeSimilarOrContainedData()
    {
        yield return new object?[]
        {
            "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
            new CoreRecognizerResult("CREDIT_CARD", new TextSpan(17, 36), 0.8),
            new CoreRecognizerResult("URL", new TextSpan(32, 40), 0.8),
            ConflictResolutionStrategy.MergeSimilarOrContained,
            "Fake card number 4151 3217 6243 34483448.com that overlaps with nonexisting URL.",
        };

        yield return new object?[]
        {
            "Fake text with SSN 145-45-6789 and phone number 953-555-5555.",
            new CoreRecognizerResult("SSN", new TextSpan(19, 30), 0.85),
            new CoreRecognizerResult("PHONE_NUMBER", new TextSpan(48, 60), 0.95),
            null,
            "Fake text with SSN 145-45-6789 and phone number 953-555-5555.",
        };
    }

    [Theory]
    [MemberData(nameof(MergeSimilarOrContainedData))]
    public void MergeSimilarOrContainedStrategyProducesExpectedText(
        string text,
        CoreRecognizerResult first,
        CoreRecognizerResult second,
        ConflictResolutionStrategy? strategy,
        string expected)
    {
        var engine = new AnonymizerEngine();
        var opConfig = new OperatorConfig("keep");
        var result = engine.Anonymize(
            text,
            new[] { first, second },
            new Dictionary<string, OperatorConfig> { ["DEFAULT"] = opConfig },
            strategy ?? ConflictResolutionStrategy.MergeSimilarOrContained);

        Assert.Equal(expected, result.Text);
    }

    public static IEnumerable<object?[]> RemoveIntersectionsData()
    {
        yield return new object?[]
        {
            "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
            new List<CoreRecognizerResult>
            {
                new("CREDIT_CARD", new TextSpan(17, 36), 1.0),
                new("URL", new TextSpan(32, 40), 0.5),
            },
            new EngineResult(
                "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
                new List<OperatorResult>
                {
                    new(17, 36, "CREDIT_CARD", "4151 3217 6243 3448", "keep"),
                    new(36, 40, "URL", ".com", "keep"),
                })
        };

        yield return new object?[]
        {
            "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
            new List<CoreRecognizerResult>
            {
                new("CREDIT_CARD", new TextSpan(17, 36), 0.8),
                new("URL", new TextSpan(32, 40), 1.0),
            },
            new EngineResult(
                "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
                new List<OperatorResult>
                {
                    new(17, 32, "CREDIT_CARD", "4151 3217 6243 ", "keep"),
                    new(32, 40, "URL", "3448.com", "keep"),
                })
        };

        yield return new object?[]
        {
            "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
            new List<CoreRecognizerResult>
            {
                new("CREDIT_CARD", new TextSpan(17, 36), 0.8),
                new("URL", new TextSpan(32, 40), 0.8),
            },
            new EngineResult(
                "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
                new List<OperatorResult>
                {
                    new(17, 36, "CREDIT_CARD", "4151 3217 6243 3448", "keep"),
                    new(36, 40, "URL", ".com", "keep"),
                })
        };

        yield return new object?[]
        {
            "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
            new List<CoreRecognizerResult>
            {
                new("CREDIT_CARD", new TextSpan(17, 36), 0.8),
                new("URL", new TextSpan(28, 40), 0.8),
                new("Ent1", new TextSpan(31, 42), 0.9),
                new("Ent2", new TextSpan(25, 40), 0.8),
            },
            new EngineResult(
                "Fake card number 4151 3217 6243 3448.com that overlaps with nonexisting URL.",
                new List<OperatorResult>
                {
                    new(31, 42, "Ent1", " 3448.com t", "keep"),
                    new(17, 31, "CREDIT_CARD", "4151 3217 6243", "keep"),
                })
        };
    }

    [Theory]
    [MemberData(nameof(RemoveIntersectionsData))]
    public void RemoveIntersectionsStrategyNormalizesConflicts(
        string text,
        List<CoreRecognizerResult> analyzerResults,
        EngineResult expected)
    {
        var engine = new AnonymizerEngine();
        var result = engine.Anonymize(
            text,
            analyzerResults,
            new Dictionary<string, OperatorConfig> { ["DEFAULT"] = new OperatorConfig("keep") },
            ConflictResolutionStrategy.RemoveIntersections);

        Assert.Equal(expected.Text, result.Text);
        Assert.Equal(expected.Items.OrderBy(item => item.Start), result.Items.OrderBy(item => item.Start));
    }
}
