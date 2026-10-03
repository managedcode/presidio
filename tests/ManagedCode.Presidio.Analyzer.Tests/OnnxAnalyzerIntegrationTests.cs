using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class OnnxAnalyzerIntegrationTests
{
    private static readonly string[] PersonEntities = { "PERSON" };
    private static readonly string[] ExactAllowList = { "Sharon" };
    private static readonly string[] RegexAllowList = { "sharon", "seattle" };
    private static readonly string[] NoMatchAllowList = { "Donna" };
    private static readonly string[] AllEntitiesAllowList = { "Sharon", "Seattle" };

    [LocalOnnxFact]
    public void OnnxModelRecognizesPersonAndLocationFromAnalyzerScenario()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldContain(r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [LocalOnnxFact]
    public void OnnxModelRecognizesPersonAndLocationFromSecondSample()
    {
        const string sample = "Barack Obama was born in Hawaii.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldContain(r => r.EntityType == "PERSON" && Slice(sample, r).Equals("Barack Obama", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => r.EntityType == "LOCATION" && Slice(sample, r).Equals("Hawaii", StringComparison.OrdinalIgnoreCase));
    }

    [LocalOnnxFact]
    public void OnnxModelFiltersToRequestedEntities()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", PersonEntities).ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.EntityType.ShouldBe("PERSON");
        }

        results.ShouldContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldNotContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [LocalOnnxFact]
    public void OnnxModelReturnsAllEntitiesWhenFilterIsEmpty()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", Array.Empty<string>()).ToList();

        results.ShouldContain(r => r.EntityType == "PERSON");
        results.ShouldContain(r => r.EntityType == "LOCATION");
    }

    [LocalOnnxFact]
    public void OnnxModelReturnsNoResultsForEmptyText()
    {
        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(string.Empty, "en").ToList();

        results.ShouldBeEmpty();
    }

    [LocalOnnxFact]
    public void OnnxModelThrowsForUnsupportedLanguage()
    {
        using var engine = CreateOnnxAnalyzer();

        var exception = Should.Throw<InvalidOperationException>(() => engine.Analyze("John lives in Berlin.", "de").ToList());
        exception.Message.ShouldContain("No matching recognizers", Case.Sensitive);
    }

    [LocalOnnxFact]
    public void OnnxModelHonorsExactAllowList()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", allowList: ExactAllowList).ToList();

        results.ShouldNotContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [LocalOnnxFact]
    public void OnnxModelHonorsRegexAllowList()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(
                sample,
                "en",
                allowList: RegexAllowList,
                allowListMatch: AllowListMatch.Regex,
                regexOptions: RegexOptions.IgnoreCase)
            .ToList();

        results.ShouldBeEmpty();
    }

    [LocalOnnxFact]
    public void OnnxModelRegexAllowListRespectsCaseSensitivity()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
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

    [LocalOnnxTheory]
    [InlineData("uk")]
    [InlineData("ja")]
    [InlineData("ko")]
    [InlineData("fr")]
    public void OnnxModelRejectsUnsupportedLanguages(string language)
    {
        using var engine = CreateOnnxAnalyzer();

        var exception = Should.Throw<InvalidOperationException>(() => engine.Analyze("Sample text", language).ToList());
        exception.Message.ShouldContain("No matching recognizers", Case.Sensitive);
    }

    [LocalOnnxTheory]
    [InlineData("Je m'appelle François Dupont and I live in Paris.", "Paris")]
    [InlineData("Мій колега Олександр відвідав Київ.", "Київ")]
    [InlineData("私の同僚 Sato works in Tokyo.", "Tokyo")]
    [InlineData("My friend 민준 lives in Seoul.", "Seoul")]
    public void OnnxModelDetectsLocationsWithinInternationalText(string text, string expectedLocation)
    {
        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(text, "en").ToList();

        var expectedIndex = text.IndexOf(expectedLocation, StringComparison.OrdinalIgnoreCase);
        expectedIndex.ShouldBeGreaterThanOrEqualTo(0, $"Sample text does not contain expected location '{expectedLocation}'.");

        results.ShouldContain(r => r.EntityType == "LOCATION");
    }

    [LocalOnnxFact]
    public void OnnxModelClearsAnalysisExplanationByDefault()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en").ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.AnalysisExplanation.ShouldBeNull();
        }
    }

    [LocalOnnxFact]
    public void OnnxModelRetainsAnalysisExplanationWhenRequested()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", returnDecisionProcess: true).ToList();

        results.ShouldNotBeEmpty();
        foreach (var result in results)
        {
            result.AnalysisExplanation.ShouldNotBeNull();
        }
    }

    [LocalOnnxFact]
    public void OnnxModelRespectsHighScoreThreshold()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var baseline = engine.Analyze(sample, "en").ToList();
        var strictThreshold = baseline.Max(r => r.Score) + 0.001;

        var filtered = engine.Analyze(sample, "en", scoreThreshold: strictThreshold).ToList();

        filtered.ShouldBeEmpty();
    }

    [LocalOnnxFact]
    public void OnnxModelIgnoresAllowListWhenNoMatches()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", allowList: NoMatchAllowList).ToList();

        results.ShouldContain(r => Slice(sample, r).Equals("Sharon", StringComparison.OrdinalIgnoreCase));
        results.ShouldContain(r => Slice(sample, r).Equals("Seattle", StringComparison.OrdinalIgnoreCase));
    }

    [LocalOnnxFact]
    public void OnnxModelRemovesAllEntitiesWhenAllowListCoversText()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
        var results = engine.Analyze(sample, "en", allowList: AllEntitiesAllowList).ToList();

        results.ShouldBeEmpty();
    }

    [LocalOnnxFact]
    public void OnnxModelLoadsRecognizersOnDemand()
    {
        using var engine = CreateOnnxAnalyzer();
        var recognizer = engine.GetRecognizers("en").OfType<OnnxNerRecognizer>().Single();

        recognizer.IsLoaded.ShouldBeFalse();

        _ = engine.Analyze("My name is Sharon and I live in Seattle.", "en").ToList();

        recognizer.IsLoaded.ShouldBeTrue();
    }

    [LocalOnnxFact]
    public void OnnxModelPopulatesRecognizerMetadata()
    {
        const string sample = "My name is Sharon and I live in Seattle.";

        using var engine = CreateOnnxAnalyzer();
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

    [LocalOnnxFact]
    public void OnnxModelReportsSupportedEntities()
    {
        using var engine = CreateOnnxAnalyzer();
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

    private static AnalyzerEngine CreateOnnxAnalyzer()
    {
        var modelPath = Environment.GetEnvironmentVariable("PRESIDIO_ONNX_MODEL_PATH");
        var vocabularyPath = Environment.GetEnvironmentVariable("PRESIDIO_ONNX_VOCAB_PATH");
        var configurationPath = Environment.GetEnvironmentVariable("PRESIDIO_ONNX_CONFIG_PATH");

        if (string.IsNullOrWhiteSpace(modelPath) ||
            string.IsNullOrWhiteSpace(vocabularyPath) ||
            string.IsNullOrWhiteSpace(configurationPath))
        {
            throw new InvalidOperationException(
                "Configure all three local ONNX artifact paths: PRESIDIO_ONNX_MODEL_PATH, PRESIDIO_ONNX_VOCAB_PATH, and PRESIDIO_ONNX_CONFIG_PATH.");
        }

        if (!File.Exists(modelPath) || !File.Exists(vocabularyPath) || !File.Exists(configurationPath))
        {
            throw new FileNotFoundException(
                "A configured local ONNX integration artifact does not exist. Check PRESIDIO_ONNX_MODEL_PATH, PRESIDIO_ONNX_VOCAB_PATH, and PRESIDIO_ONNX_CONFIG_PATH.");
        }

        var engine = new OnnxNlpEngine(new OnnxNlpEngineOptions
        {
            ModelPath = modelPath,
            VocabularyPath = vocabularyPath,
            ConfigurationPath = configurationPath,
        });

        return new AnalyzerEngine(nlpEngine: engine);
    }
}

public sealed class LocalOnnxFactAttribute : FactAttribute
{
    public LocalOnnxFactAttribute()
    {
        if (!LocalOnnxArtifacts.IsConfigured())
        {
            Skip = LocalOnnxArtifacts.SkipReason;
        }
    }
}

public sealed class LocalOnnxTheoryAttribute : TheoryAttribute
{
    public LocalOnnxTheoryAttribute()
    {
        if (!LocalOnnxArtifacts.IsConfigured())
        {
            Skip = LocalOnnxArtifacts.SkipReason;
        }
    }
}

internal static class LocalOnnxArtifacts
{
    public const string SkipReason = "Optional ONNX tests require existing local artifacts configured with PRESIDIO_ONNX_MODEL_PATH, PRESIDIO_ONNX_VOCAB_PATH, and PRESIDIO_ONNX_CONFIG_PATH.";

    public static bool IsConfigured()
    {
        var paths = new[]
        {
            Environment.GetEnvironmentVariable("PRESIDIO_ONNX_MODEL_PATH"),
            Environment.GetEnvironmentVariable("PRESIDIO_ONNX_VOCAB_PATH"),
            Environment.GetEnvironmentVariable("PRESIDIO_ONNX_CONFIG_PATH"),
        };

        return paths.Any(path => !string.IsNullOrWhiteSpace(path));
    }
}
