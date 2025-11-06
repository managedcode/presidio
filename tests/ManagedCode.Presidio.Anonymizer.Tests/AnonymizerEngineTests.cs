using ManagedCode.Presidio.Core;
using Xunit;
using CoreRecognizerResult = ManagedCode.Presidio.Core.RecognizerResult;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class AnonymizerEngineTests
{
    private static CoreRecognizerResult CreateResult(string entityType, int start, int end, double score = 0.8)
        => new(entityType, new TextSpan(start, end), score);

    [Fact]
    public void GetAnonymizersReturnsPredefinedOperators()
    {
        var engine = new AnonymizerEngine();

        var anonymizers = engine.GetAnonymizers();

        var expected = new[] { "custom", "encrypt", "hash", "keep", "mask", "redact", "replace" };
        Assert.Equal(expected.OrderBy(x => x), anonymizers.OrderBy(x => x));
    }

    [Fact]
    public void EmptyAnalyzerResultsReturnOriginalText()
    {
        var engine = new AnonymizerEngine();
        var text = "one two three";

        var result = engine.Anonymize(text, Array.Empty<CoreRecognizerResult>());

        Assert.Equal(text, result.Text);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void MissingOperatorsFallbackToDefaultReplace()
    {
        var engine = new AnonymizerEngine();
        var text = "please REPLACE ME.";
        var analyzerResult = CreateResult("SSN", 7, 17);

        var result = engine.Anonymize(text, new[] { analyzerResult });

        Assert.Equal("please <SSN>.", result.Text);
    }

    [Fact]
    public void CustomOperatorExecutesProvidedLambda()
    {
        var engine = new AnonymizerEngine();
        var text = "Fake card number 4151 3217 6243 3448.com that overlaps";
        var analyzerResults = new[]
        {
            CreateResult("CREDIT_CARD", 17, 36),
            CreateResult("URL", 32, 40),
        };

        var operators = new Dictionary<string, OperatorConfig>
        {
            {
                "DEFAULT",
                new OperatorConfig("custom", new Dictionary<string, object?>
                {
                    [CustomOperator.LambdaKey] = new Func<string, string>(value => $"<ENTITY: {value}>")
                })
            },
        };

        var result = engine.Anonymize(text, analyzerResults, operators);

        Assert.Equal("Fake card number <ENTITY: 4151 3217 6243 3448><ENTITY: 3448.com> that overlaps", result.Text);
    }

    [Fact]
    public void DefaultOperatorIsAppliedWhenProvided()
    {
        var engine = new AnonymizerEngine();
        var text = "please REPLACE ME.";
        var analyzerResult = CreateResult("SSN", 7, 17);

        var result = engine.Anonymize(
            text,
            new[] { analyzerResult },
            new Dictionary<string, OperatorConfig>
            {
                ["DEFAULT"] = new OperatorConfig("replace", new Dictionary<string, object?> { [ReplaceOperator.NewValueKey] = "and thank you" }),
            });

        Assert.Equal("please and thank you.", result.Text);
    }

    [Fact]
    public void EntitySpecificOperatorOverridesDefault()
    {
        var engine = new AnonymizerEngine();
        var text = "please REPLACE ME.";
        var analyzerResult = CreateResult("SSN", 7, 17);

        var result = engine.Anonymize(
            text,
            new[] { analyzerResult },
            new Dictionary<string, OperatorConfig>
            {
                ["DEFAULT"] = new OperatorConfig("replace", new Dictionary<string, object?> { [ReplaceOperator.NewValueKey] = "and thank you" }),
                ["SSN"] = new OperatorConfig("redact"),
            });

        Assert.Equal("please .", result.Text);
    }

    [Theory]
    [InlineData("hello world", 5, 12)]
    [InlineData("hello world", 12, 13)]
    public void InvalidIndicesThrowInvalidParamException(string text, int start, int end)
    {
        var engine = new AnonymizerEngine();
        var analyzerResult = CreateResult("TYPE", start, end);

        var exception = Assert.Throws<InvalidParamException>(() => engine.Anonymize(text, new[] { analyzerResult }));
        Assert.Equal($"Invalid analyzer result, start: {start} and end: {end}, while text length is only {text.Length}.", exception.Message);
    }

    [Fact]
    public void InvalidOperatorNameRaisesError()
    {
        var engine = new AnonymizerEngine();
        var analyzerResult = CreateResult("number", 0, 4);
        var operators = new Dictionary<string, OperatorConfig>
        {
            ["number"] = new OperatorConfig("fake"),
        };

        var exception = Assert.Throws<InvalidParamException>(() => engine.Anonymize("this is my text", new[] { analyzerResult }, operators));
        Assert.Equal("Invalid operator class 'fake'.", exception.Message);
    }

    [Fact]
    public void WhitespaceSeparatedEntitiesAreMerged()
    {
        var engine = new AnonymizerEngine();
        var text = "My name is David   Jones";
        var analyzerResults = new[]
        {
            CreateResult("PERSON", 11, 16),
            CreateResult("PERSON", 19, 24),
        };

        var operators = new Dictionary<string, OperatorConfig>
        {
            ["PERSON"] = new OperatorConfig("replace", new Dictionary<string, object?> { [ReplaceOperator.NewValueKey] = "BIP" }),
        };

        var result = engine.Anonymize(text, analyzerResults, operators);

        Assert.Equal("My name is BIP", result.Text);
        Assert.Single(result.Items);
        Assert.Equal(11, result.Items[0].Start);
        Assert.Equal(14, result.Items[0].End);
    }

    [Fact]
    public void AnalyzerInputRemainsUnchangedAfterAnonymize()
    {
        var engine = new AnonymizerEngine();
        var text = "Jane Doe is a person";
        var analyzerResults = new List<CoreRecognizerResult>
        {
            CreateResult("PERSON", 0, 4, 1.0),
            CreateResult("PERSON", 5, 8, 1.0),
        };
        var snapshot = analyzerResults
            .Select(result => (entityType: result.EntityType, start: result.Start, end: result.End, score: result.Score))
            .ToList();

        _ = engine.Anonymize(text, analyzerResults);

        for (var i = 0; i < analyzerResults.Count; i++)
        {
            Assert.Equal(snapshot[i].start, analyzerResults[i].Start);
            Assert.Equal(snapshot[i].end, analyzerResults[i].End);
            Assert.Equal(snapshot[i].entityType, analyzerResults[i].EntityType);
            Assert.Equal(snapshot[i].score, analyzerResults[i].Score);
        }
    }

    [Fact]
    public void UnsortedAnalyzerResultsAreMergedCorrectly()
    {
        var engine = new AnonymizerEngine();
        var text = "Jane Doe is a person";
        var analyzerResults = new[]
        {
            CreateResult("PERSON", 5, 8, 1.0),
            CreateResult("PERSON", 0, 4, 1.0),
        };

        var result = engine.Anonymize(text, analyzerResults);

        Assert.Equal("<PERSON> is a person", result.Text);
    }

    [Fact]
    public void ConflictResolutionRemoveIntersectionsAdjustsSpans()
    {
        var engine = new AnonymizerEngine();
        var text = "Call me at 555-1234";
        var analyzerResults = new[]
        {
            CreateResult("PHONE_NUMBER", 11, 19, 0.6),
            CreateResult("NUMBER", 15, 19, 0.9),
        };

        var result = engine.Anonymize(
            text,
            analyzerResults,
            new Dictionary<string, OperatorConfig>
            {
                ["DEFAULT"] = new OperatorConfig("redact"),
            },
            ConflictResolutionStrategy.RemoveIntersections);

        Assert.Equal("Call me at ", result.Text);
        Assert.Single(result.Items);
    }
}
