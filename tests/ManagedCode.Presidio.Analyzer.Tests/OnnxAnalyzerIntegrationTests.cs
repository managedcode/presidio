using System.Text.RegularExpressions;
using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class OnnxAnalyzerIntegrationTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string[] PersonEntities = { "PERSON" };
    private static readonly string[] ExactAllowList = { "Sharon" };
    private static readonly string[] RegexAllowList = { "sharon", "seattle" };
    private static readonly string[] NoMatchAllowList = { "Donna" };
    private static readonly string[] AllEntitiesAllowList = { "Sharon", "Seattle" };

    [Fact]
    public void OnnxModelRecognizesPersonAndLocationFromPythonTestFixture()
    {
        var pythonTestPath = Path.Combine(RepoRoot, "external", "microsoft-presidio", "presidio-analyzer", "tests", "test_analyzer_engine.py");
        Assert.True(File.Exists(pythonTestPath));

        var pythonContent = File.ReadAllText(pythonTestPath);
        const string sample = "My name is Sharon and I live in Seattle.";
        Assert.Contains(sample, pythonContent, StringComparison.Ordinal);

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        Assert.Contains(results, r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(results, r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelRecognizesPersonAndLocationFromStanzaSample()
    {
        var pythonTestPath = Path.Combine(RepoRoot, "external", "microsoft-presidio", "presidio-analyzer", "tests", "test_stanza_nlp_engine.py");
        Assert.True(File.Exists(pythonTestPath));

        var pythonContent = File.ReadAllText(pythonTestPath);
        const string sample = "Barack Obama was born in Hawaii.";
        Assert.Contains(sample, pythonContent, StringComparison.Ordinal);

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        Assert.Contains(results, r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Barack Obama", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(results, r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Hawaii", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelFiltersToRequestedEntities()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", PersonEntities).ToList();

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal("PERSON", r.EntityType));
        Assert.Contains(results, r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(results, r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelReturnsAllEntitiesWhenFilterIsEmpty()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", Array.Empty<string>()).ToList();

        Assert.Contains(results, r => r.EntityType == "PERSON");
        Assert.Contains(results, r => r.EntityType == "LOCATION");
    }

    [Fact]
    public void OnnxModelReturnsNoResultsForEmptyText()
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(string.Empty, "en").ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void OnnxModelThrowsForUnsupportedLanguage()
    {
        using var engine = new AnalyzerEngine();

        var exception = Assert.Throws<InvalidOperationException>(() => engine.Analyze("John lives in Berlin.", "de").ToList());
        Assert.Contains("No matching recognizers", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OnnxModelHonorsExactAllowList()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: ExactAllowList).ToList();

        Assert.DoesNotContain(results, r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(results, r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelHonorsRegexAllowList()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(
                sample,
                "en",
                allowList: RegexAllowList,
                allowListMatch: AllowListMatch.Regex,
                regexOptions: RegexOptions.IgnoreCase)
            .ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void OnnxModelClearsAnalysisExplanationByDefault()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Null(r.AnalysisExplanation));
    }

    [Fact]
    public void OnnxModelRetainsAnalysisExplanationWhenRequested()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", returnDecisionProcess: true).ToList();

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.NotNull(r.AnalysisExplanation));
    }

    [Fact]
    public void OnnxModelRespectsHighScoreThreshold()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var baseline = engine.Analyze(sample, "en").ToList();
        var strictThreshold = baseline.Max(r => r.Score) + 0.001;

        var filtered = engine.Analyze(sample, "en", scoreThreshold: strictThreshold).ToList();

        Assert.Empty(filtered);
    }

    [Fact]
    public void OnnxModelIgnoresAllowListWhenNoMatches()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: NoMatchAllowList).ToList();

        Assert.Contains(results, r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(results, r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelRemovesAllEntitiesWhenAllowListCoversText()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: AllEntitiesAllowList).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void OnnxModelLoadsRecognizersOnDemand()
    {
        using var engine = new AnalyzerEngine();
        var recognizer = engine.GetRecognizers("en").OfType<OnnxNerRecognizer>().Single();

        Assert.False(recognizer.IsLoaded);

        _ = engine.Analyze("My name is Sharon and I live in Seattle.", "en").ToList();

        Assert.True(recognizer.IsLoaded);
    }

    [Fact]
    public void OnnxModelPopulatesRecognizerMetadata()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", returnDecisionProcess: true).ToList();

        Assert.NotEmpty(results);
        var person = results.First(result => result.EntityType == "PERSON");

        Assert.True(person.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerIdentifier, out var identifier));
        Assert.False(string.IsNullOrWhiteSpace(identifier));

        Assert.True(person.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerName, out var recognizerName));
        Assert.Equal("OnnxNerRecognizer", recognizerName);

        Assert.True(person.Metadata.ContainsKey("raw_label"));
        Assert.True(person.Metadata.ContainsKey("source"));
    }

    [Fact]
    public void OnnxModelReportsSupportedEntities()
    {
        using var engine = new AnalyzerEngine();
        var supported = engine.GetSupportedEntities("en");

        Assert.Contains("PERSON", supported);
        Assert.Contains("LOCATION", supported);
    }

    private static string Slice(string text, RecognizerResult result)
    {
        if (result.Start < 0 || result.End > text.Length || result.End <= result.Start)
        {
            return string.Empty;
        }

        return text[result.Start..result.End];
    }

    private static string FindRepoRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            if (File.Exists(Path.Combine(directory, "Presidio.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Unable to locate repository root from test base directory.");
    }
}
