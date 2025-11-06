using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class BatchAnonymizerEngineTests
{
    private static RecognizerResult PersonEntity => new("PERSON", 0, 4, 0.85);

    [Fact]
    public void AnonymizeDictReplacesValues()
    {
        var texts = new[] { "John", "Jill", "Jack" };

        _ = new[] { PersonEntity, PersonEntity, PersonEntity };

        var engine = new BatchAnonymizerEngine();
        var input = new[]
        {
            new DictRecognizerResult(
                "name",
                texts,
                new List<IReadOnlyList<RecognizerResult>>
                {
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                })
        };

        var result = engine.AnonymizeDict(input);

        var names = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result["name"]);
        Assert.Equal(new[] { "<PERSON>", "<PERSON>", "<PERSON>" }, names);
    }

    [Fact]
    public void AnonymizeListRespectsNonStringValues()
    {
        var engine = new BatchAnonymizerEngine();
        var texts = new object?[] { "John", "Jill", "Jack", new object[] { "random", 123, true } };
        var recognizerResults = new List<IReadOnlyList<RecognizerResult>>
        {
            new List<RecognizerResult> { PersonEntity },
            new List<RecognizerResult> { PersonEntity },
            new List<RecognizerResult> { PersonEntity },
            Array.Empty<RecognizerResult>(),
        };

        var result = engine.AnonymizeList(texts, recognizerResults);

        Assert.Equal("<PERSON>", result[0]);
        Assert.Equal("<PERSON>", result[1]);
        Assert.Equal("<PERSON>", result[2]);
        Assert.Equal(texts[3], result[3]);
    }

    [Fact]
    public void EmptyRecognizerResultsLeaveTextUnchanged()
    {
        var engine = new BatchAnonymizerEngine();
        var texts = new object?[] { "John", "Jill", "Jack" };
        var result = engine.AnonymizeDict(new[]
        {
            new DictRecognizerResult(
                "name",
                texts,
                Array.Empty<IReadOnlyList<RecognizerResult>>())
        });

        var values = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result["name"]);
        Assert.Equal(texts, values);
    }

    [Fact]
    public void ComplexDictionaryAnonymizesExpectedValues()
    {
        var engine = new BatchAnonymizerEngine();
        var analyzerResults = new[]
        {
            new DictRecognizerResult(
                "name",
                new[] { "John", "Jill", "Jack" },
                new List<IReadOnlyList<RecognizerResult>>
                {
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                }),
            new DictRecognizerResult(
                "comments",
                new[]
                {
                    "called him yesterday to confirm he requested to call back in 2 days",
                    "accepted the offer license number AC432223",
                    "need to call him at phone number 212-555-5555",
                },
                new List<IReadOnlyList<RecognizerResult>>
                {
                    new List<RecognizerResult>
                    {
                        new("DATE_TIME", 11, 20, 0.85),
                        new("DATE_TIME", 61, 67, 0.85),
                    },
                    new List<RecognizerResult>
                    {
                        new("US_DRIVER_LICENSE", 34, 42, 0.65),
                    },
                    new List<RecognizerResult>
                    {
                        new("PHONE_NUMBER", 33, 45, 0.75),
                    },
                }),
        };

        var result = engine.AnonymizeDict(analyzerResults);
        var names = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result["name"]);
        Assert.All(names, value => Assert.Equal("<PERSON>", value));

        var comments = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result["comments"]);
        Assert.Equal(
            "called him <DATE_TIME> to confirm he requested to call back in <DATE_TIME>",
            comments[0]);
        Assert.Equal("accepted the offer license number <US_DRIVER_LICENSE>", comments[1]);
        Assert.Equal("need to call him at phone number <PHONE_NUMBER>", comments[2]);
    }

    [Fact]
    public void DictionaryValueCanContainNestedDictionary()
    {
        var engine = new BatchAnonymizerEngine();
        var nestedResults = new List<DictRecognizerResult>
        {
            new("name", "John", new List<RecognizerResult> { PersonEntity }),
        };

        var analyzerResults = new[]
        {
            new DictRecognizerResult(
                "customer",
                new Dictionary<string, object?> { ["name"] = "John" },
                nestedResults)
        };

        var result = engine.AnonymizeDict(analyzerResults);
        var customer = Assert.IsAssignableFrom<IDictionary<string, object?>>(result["customer"]);
        Assert.Equal("<PERSON>", customer["name"]);
    }

    [Fact]
    public void DictionaryValueWithNonTextReturnsOriginal()
    {
        var engine = new BatchAnonymizerEngine();
        var analyzerResults = new[]
        {
            new DictRecognizerResult("id", 123, Array.Empty<IReadOnlyList<RecognizerResult>>())
        };

        var result = engine.AnonymizeDict(analyzerResults);
        Assert.Equal(123, result["id"]);
    }

    [Fact]
    public void CustomOperatorAppliedToDictValues()
    {
        var engine = new BatchAnonymizerEngine();
        var texts = new[] { "John", "Jill", "Jack" };
        var analyzerResults = new[]
        {
            new DictRecognizerResult(
                "name",
                texts,
                new List<IReadOnlyList<RecognizerResult>>
                {
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                    new List<RecognizerResult> { PersonEntity },
                })
        };

        var config = new OperatorConfig("custom", new Dictionary<string, object?>
        {
            [CustomOperator.LambdaKey] = new Func<string, string>(value => $"<ENTITY: {value}>")
        });

        var result = engine.AnonymizeDict(analyzerResults, new Dictionary<string, OperatorConfig>
        {
            ["DEFAULT"] = config
        });

        var customNames = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result["name"]);
        Assert.Equal(new[] { "<ENTITY: John>", "<ENTITY: Jill>", "<ENTITY: Jack>" }, customNames);
    }
}
