using System.Text.RegularExpressions;
using Shouldly;
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
        File.Exists(pythonTestPath).ShouldBeTrue();

        var pythonContent = File.ReadAllText(pythonTestPath);
        const string sample = "My name is Sharon and I live in Seattle.";
        pythonContent.ShouldContain(sample);

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldContain(r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelRecognizesPersonAndLocationFromStanzaSample()
    {
        var pythonTestPath = Path.Combine(RepoRoot, "external", "microsoft-presidio", "presidio-analyzer", "tests", "test_stanza_nlp_engine.py");
        File.Exists(pythonTestPath).ShouldBeTrue();

        var pythonContent = File.ReadAllText(pythonTestPath);
        const string sample = "Barack Obama was born in Hawaii.";
        pythonContent.ShouldContain(sample);

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldContain(r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Barack Obama", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Hawaii", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelFiltersToRequestedEntities()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", PersonEntities).ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.EntityType.ShouldBe("PERSON");
        }

        results.ShouldContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldNotContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelReturnsAllEntitiesWhenFilterIsEmpty()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", Array.Empty<string>()).ToList();

        results.ShouldContain(r => r.EntityType == "PERSON");
        results.ShouldContain(r => r.EntityType == "LOCATION");
    }

    [Fact]
    public void OnnxModelReturnsNoResultsForEmptyText()
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(string.Empty, "en").ToList();

        results.ShouldBeEmpty();
    }

    [Fact]
    public void OnnxModelThrowsForUnsupportedLanguage()
    {
        using var engine = new AnalyzerEngine();

        var exception = Should.Throw<InvalidOperationException>(() => engine.Analyze("John lives in Berlin.", "de").ToList());
        exception.Message.ShouldContain("No matching recognizers", Case.Sensitive);
    }

    [Fact]
    public void OnnxModelHonorsExactAllowList()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: ExactAllowList).ToList();

        results.ShouldNotContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
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

        results.ShouldBeEmpty();
    }

    [Fact]
    public void OnnxModelRegexAllowListRespectsCaseSensitivity()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var insensitive = engine.Analyze(
                sample,
                "en",
                allowList: new[] { "SHARON", "SEATTLE" },
                allowListMatch: AllowListMatch.Regex,
                regexOptions: RegexOptions.IgnoreCase)
            .ToList();

        insensitive.ShouldBeEmpty();

        var sensitive = engine.Analyze(
                sample,
                "en",
                allowList: new[] { "SHARON", "SEATTLE" },
                allowListMatch: AllowListMatch.Regex,
                regexOptions: RegexOptions.None)
            .ToList();

        sensitive.ShouldContain(r => Slice(sample, r).Equals("Sharon", StringComparison.Ordinal));
        sensitive.ShouldContain(r => Slice(sample, r).Equals("Seattle", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("uk")]
    [InlineData("ja")]
    [InlineData("ko")]
    [InlineData("fr")]
    public void OnnxModelRejectsUnsupportedLanguages(string language)
    {
        using var engine = new AnalyzerEngine();

        var exception = Should.Throw<InvalidOperationException>(() => engine.Analyze("Sample text", language).ToList());
        exception.Message.ShouldContain("No matching recognizers", Case.Sensitive);
    }

    [Theory]
    [InlineData("Je m'appelle François Dupont and I live in Paris.", "Paris")]
    [InlineData("Мій колега Олександр відвідав Київ.", "Київ")]
    [InlineData("私の同僚 Sato works in Tokyo.", "Tokyo")]
    [InlineData("My friend 민준 lives in Seoul.", "Seoul")]
    public void OnnxModelDetectsLocationsWithinInternationalText(string text, string expectedLocation)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en").ToList();

        var expectedIndex = text.IndexOf(expectedLocation, StringComparison.OrdinalIgnoreCase);
        expectedIndex.ShouldBeGreaterThanOrEqualTo(0, $"Sample text does not contain expected location '{expectedLocation}'.");

        results.ShouldContain(r => r.EntityType == "LOCATION");
    }

    [Fact]
    public void OnnxModelClearsAnalysisExplanationByDefault()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.AnalysisExplanation.ShouldBeNull();
        }
    }

    [Fact]
    public void OnnxModelRetainsAnalysisExplanationWhenRequested()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", returnDecisionProcess: true).ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.AnalysisExplanation.ShouldNotBeNull();
        }
    }

    [Fact]
    public void OnnxModelRespectsHighScoreThreshold()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var baseline = engine.Analyze(sample, "en").ToList();
        var strictThreshold = baseline.Max(r => r.Score) + 0.001;

        var filtered = engine.Analyze(sample, "en", scoreThreshold: strictThreshold).ToList();

        filtered.ShouldBeEmpty();
    }

    [Fact]
    public void OnnxModelIgnoresAllowListWhenNoMatches()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: NoMatchAllowList).ToList();

        results.ShouldContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnnxModelRemovesAllEntitiesWhenAllowListCoversText()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", allowList: AllEntitiesAllowList).ToList();

        results.ShouldBeEmpty();
    }

    [Fact]
    public void OnnxModelLoadsRecognizersOnDemand()
    {
        using var engine = new AnalyzerEngine();
        var recognizer = engine.GetRecognizers("en").OfType<OnnxNerRecognizer>().Single();

        recognizer.IsLoaded.ShouldBeFalse();

        _ = engine.Analyze("My name is Sharon and I live in Seattle.", "en").ToList();

        recognizer.IsLoaded.ShouldBeTrue();
    }

    [Fact]
    public void OnnxModelPopulatesRecognizerMetadata()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(sample, "en", returnDecisionProcess: true).ToList();

        results.ShouldNotBeEmpty();
        var person = results.First(result => result.EntityType == "PERSON");

        person.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerIdentifier, out var identifier).ShouldBeTrue();
        identifier.ShouldNotBeNullOrWhiteSpace();

        person.TryGetMetadata<string>(RecognitionMetadataKeys.RecognizerName, out var recognizerName).ShouldBeTrue();
        recognizerName.ShouldBe("OnnxNerRecognizer");

        person.Metadata.ContainsKey("raw_label").ShouldBeTrue();
        person.Metadata.ContainsKey("source").ShouldBeTrue();
    }

    [Fact]
    public void OnnxModelReportsSupportedEntities()
    {
        using var engine = new AnalyzerEngine();
        var supported = engine.GetSupportedEntities("en");

        supported.ShouldContain("PERSON");
        supported.ShouldContain("LOCATION");
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
            if (File.Exists(Path.Combine(directory, "Presidio.sln")) ||
                File.Exists(Path.Combine(directory, "Presidio.slnx")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Unable to locate repository root from test base directory.");
    }
}
