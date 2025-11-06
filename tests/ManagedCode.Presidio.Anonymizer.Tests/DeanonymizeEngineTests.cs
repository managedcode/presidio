using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class DeanonymizeEngineTests
{
    [Fact]
    public void DecryptOperatorRestoresPlainText()
    {
        const string cipherText = "My name is S184CMt9Drj7QaKQ21JTrpYzghnboTF9pn/neN8JME0=";
        var entities = new List<OperatorResult>
        {
            new(11, 55, "PERSON"),
        };

        var engine = new DeanonymizeEngine();
        var result = engine.Deanonymize(
            cipherText,
            entities,
            new Dictionary<string, OperatorConfig>
            {
                ["DEFAULT"] = new OperatorConfig("decrypt", new Dictionary<string, object?>
                {
                    [EncryptOperator.KeyParameter] = "WmZq4t7w!z%C&F)J",
                })
            });

        Assert.Equal("My name is Chloë", result.Text);
        Assert.Single(result.Items);
        Assert.Equal("Chloë", result.Items[0].Text);
        Assert.Equal(11, result.Items[0].Start);
        Assert.Equal(16, result.Items[0].End);
        Assert.Equal("PERSON", result.Items[0].EntityType);
    }

    [Fact]
    public void EmptyPayloadReturnsUnchangedText()
    {
        var engine = new DeanonymizeEngine();
        var result = engine.Deanonymize(
            string.Empty,
            Array.Empty<OperatorResult>(),
            new Dictionary<string, OperatorConfig>
            {
                ["DEFAULT"] = new OperatorConfig("decrypt", new Dictionary<string, object?>
                {
                    [EncryptOperator.KeyParameter] = "WmZq4t7w!z%C&F)J",
                })
            });

        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void InvalidKeyLengthThrows()
    {
        const string cipherText = "My name is S184CMt9Drj7QaKQ21JTrpYzghnboTF9pn/neN8JME0=";
        var entities = new List<OperatorResult> { new(11, 55, "PERSON") };
        var engine = new DeanonymizeEngine();

        var ex = Assert.Throws<InvalidParamException>(() => engine.Deanonymize(
            cipherText,
            entities,
            new Dictionary<string, OperatorConfig>
            {
                ["PERSON"] = new OperatorConfig("decrypt", new Dictionary<string, object?>
                {
                    [EncryptOperator.KeyParameter] = "1234",
                })
            }));

        Assert.Equal("Invalid input, key must be of length 128, 192 or 256 bits", ex.Message);
    }

    [Fact]
    public void EncryptThenDecryptRoundTrips()
    {
        const string key = "WmZq4t7w!z%C&F)J";
        const string text = "My name is Chloë";
        var recognizerResults = new List<RecognizerResult> { new("PERSON", 11, 16, 0.8) };

        var anonymizerEngine = new AnonymizerEngine();
        var anonymized = anonymizerEngine.Anonymize(
            text,
            recognizerResults,
            new Dictionary<string, OperatorConfig>
            {
                ["PERSON"] = new OperatorConfig("encrypt", new Dictionary<string, object?>
                {
                    [EncryptOperator.KeyParameter] = key,
                })
            });

        var deanonymizeEngine = new DeanonymizeEngine();
        var deanonymized = deanonymizeEngine.Deanonymize(
            anonymized.Text!,
            anonymized.Items,
            new Dictionary<string, OperatorConfig>
            {
                ["PERSON"] = new OperatorConfig("decrypt", new Dictionary<string, object?>
                {
                    [EncryptOperator.KeyParameter] = key,
                })
            });

        Assert.Equal(text, deanonymized.Text);
    }

    [Fact]
    public void GetDeanonymizersReturnsDefaultOperators()
    {
        var engine = new DeanonymizeEngine();
        var deanonymizers = engine.GetDeanonymizers();

        Assert.Contains("deanonymize_keep", deanonymizers);
        Assert.Contains("decrypt", deanonymizers);
    }

    [Fact]
    public void AddAndRemoveDeanonymizersUpdatesRegistry()
    {
        var engine = new DeanonymizeEngine();
        var initialCount = engine.GetDeanonymizers().Count;

        engine.AddDeanonymizer(typeof(CustomDeanonymizer));
        Assert.Equal(initialCount + 1, engine.GetDeanonymizers().Count);

        engine.RemoveDeanonymizer(typeof(DeanonymizeKeepOperator));
        Assert.Equal(initialCount, engine.GetDeanonymizers().Count);
    }

    [Fact]
    public void CustomOperatorRoundTrip()
    {
        const string text = "hello";
        var recognizerResults = new List<RecognizerResult> { new("WORD", 0, 5, 0.8) };

        var anonymizer = new AnonymizerEngine();
        anonymizer.AddAnonymizer(typeof(ReverserAnonymizeOperator));
        var anonymized = anonymizer.Anonymize(
            text,
            recognizerResults,
            new Dictionary<string, OperatorConfig>
            {
                ["WORD"] = new OperatorConfig("Reverser")
            });

        Assert.Equal("olleh", anonymized.Text);

        var deanonymizer = new DeanonymizeEngine();
        deanonymizer.AddDeanonymizer(typeof(ReverserDeanonymizeOperator));
        var restored = deanonymizer.Deanonymize(
            anonymized.Text!,
            anonymized.Items,
            new Dictionary<string, OperatorConfig>
            {
                ["WORD"] = new OperatorConfig("Reverser")
            });

        Assert.Equal(text, restored.Text);
    }

    private sealed class CustomDeanonymizer : Operator
    {
        public override string Operate(string text, IDictionary<string, object?>? parameters = null) => text;

        public override void Validate(IDictionary<string, object?>? parameters = null)
        {
        }

        public override string OperatorName => "custom_deanonymizer";

        public override OperatorType Type => OperatorType.Deanonymize;
    }

    private sealed class ReverserAnonymizeOperator : Operator
    {
        public override string Operate(string text, IDictionary<string, object?>? parameters = null) => new(text.Reverse().ToArray());

        public override void Validate(IDictionary<string, object?>? parameters = null)
        {
        }

        public override string OperatorName => "Reverser";

        public override OperatorType Type => OperatorType.Anonymize;
    }

    private sealed class ReverserDeanonymizeOperator : Operator
    {
        public override string Operate(string text, IDictionary<string, object?>? parameters = null) => new(text.Reverse().ToArray());

        public override void Validate(IDictionary<string, object?>? parameters = null)
        {
        }

        public override string OperatorName => "Reverser";

        public override OperatorType Type => OperatorType.Deanonymize;
    }
}
