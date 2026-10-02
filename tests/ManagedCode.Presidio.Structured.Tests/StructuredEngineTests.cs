using System.Text.Json.Nodes;
using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;
using Xunit;

namespace ManagedCode.Presidio.Structured.Tests;

public sealed class StructuredEngineTests
{
    [Fact]
    public void AnalyzeAndAnonymizeJsonEmailFieldThroughRealPipelines()
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());
        var input = JsonNode.Parse("""{"customer":{"email":"alice@example.org"},"note":"call tomorrow"}""")!;

        var analysis = engine.Analyze(input, "en", scoreThreshold: 0.5);
        var anonymized = engine.Anonymize(
            input,
            analysis,
            new Dictionary<string, OperatorConfig>
            {
                ["EMAIL_ADDRESS"] = new("redact"),
            });

        Assert.Equal("EMAIL_ADDRESS", analysis.EntityMapping["/customer/email"]);
        Assert.Empty(anonymized["customer"]!["email"]!.GetValue<string>());
        Assert.Equal("call tomorrow", anonymized["note"]!.GetValue<string>());
        Assert.Equal("alice@example.org", input["customer"]!["email"]!.GetValue<string>());
    }

    [Fact]
    public void AnalysisContainsNoMappingWhenInputHasNoSelectedIdentifiers()
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());
        var input = JsonNode.Parse("""{"message":"nothing to redact"}""")!;

        Assert.Empty(engine.Analyze(input, "en").EntityMapping);
    }

    [Fact]
    public void NestedStringArraysAreAnalyzedAndTransformedWithoutChangingInput()
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());
        var input = JsonNode.Parse("""{"contacts":[{"email":"alice@example.org"},{"email":"bob@example.com"}]}""")!;

        var analysis = engine.Analyze(input, "en", scoreThreshold: 0.5);
        var anonymized = engine.Anonymize(
            input,
            analysis,
            new Dictionary<string, OperatorConfig> { ["DEFAULT"] = new("redact") });

        Assert.Equal(new[] { "/contacts/0/email", "/contacts/1/email" }, analysis.EntityMapping.Keys.Order());
        Assert.Empty(anonymized["contacts"]![0]!["email"]!.GetValue<string>());
        Assert.Empty(anonymized["contacts"]![1]!["email"]!.GetValue<string>());
        Assert.Equal("alice@example.org", input["contacts"]![0]!["email"]!.GetValue<string>());
    }

    [Fact]
    public void JsonReaderRejectsMalformedInput()
    {
        var reader = new JsonDataReader();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{ malformed"));

        Assert.ThrowsAny<System.Text.Json.JsonException>(() => reader.Read(stream));
    }

    [Fact]
    public void AnonymizationFailsForAStaleAnalysisPath()
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());
        var input = JsonNode.Parse("""{"customer":{}}""")!;
        var analysis = new StructuredAnalysis(new Dictionary<string, string> { ["/customer/email"] = "EMAIL_ADDRESS" });

        Assert.Throws<InvalidOperationException>(() => engine.Anonymize(
            input,
            analysis,
            new Dictionary<string, OperatorConfig> { ["DEFAULT"] = new("redact") }));
    }

    [Fact]
    public void AnalyzeAndAnonymizeDistinguishesLiteralDotsFromNestedPropertyPaths()
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());
        var input = JsonNode.Parse("""{"a.b":"alice@example.org","a":{"b":"bob@example.net"}}""")!;

        var analysis = engine.Analyze(input, "en", scoreThreshold: 0.5);
        var anonymized = engine.Anonymize(
            input,
            analysis,
            new Dictionary<string, OperatorConfig> { ["DEFAULT"] = new("redact") });

        Assert.Equal(new[] { "/a.b", "/a/b" }, analysis.EntityMapping.Keys.Order());
        Assert.Empty(anonymized["a.b"]!.GetValue<string>());
        Assert.Empty(anonymized["a"]!["b"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("\"alice@example.org\"")]
    [InlineData("42")]
    [InlineData("true")]
    public void AnalyzeRejectsPrimitiveJsonRoots(string json)
    {
        using var analyzer = new AnalyzerEngine();
        var engine = new StructuredEngine(analyzer, new AnonymizerEngine());

        Assert.Throws<NotSupportedException>(() => engine.Analyze(JsonNode.Parse(json)!, "en"));
    }
}
