using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class EntityRecognizerTests
{
    private static readonly string[] _personEntities = { "PERSON" };

    [Fact]
    public void AnalyzeTriggersSingleLoadAndProducesResults()
    {
        var recognizer = new TestRecognizer();

        Assert.False(recognizer.IsLoaded);
        var results = recognizer.Analyze("call me", _personEntities, new NlpArtifacts("en"));

        Assert.True(recognizer.IsLoaded);
        Assert.Equal("TestRecognizer", recognizer.Name);
        Assert.Contains("PERSON", recognizer.SupportedEntities);
        Assert.Single(results);
        Assert.Equal("PERSON", results.First().EntityType);
        Assert.Equal(0, results.First().Start);
        Assert.Equal("call me".Length, results.First().End);
        Assert.Equal(1, recognizer.LoadCount);
    }

    [Fact]
    public void EnhanceUsingContextReturnsOriginalResultsByDefault()
    {
        var recognizer = new TestRecognizer();
        var results = new[]
        {
            new RecognizerResult("PERSON", new TextSpan(0, 3), 0.8),
        };

        var enhanced = recognizer.EnhanceUsingContext(
            "abc",
            results,
            Array.Empty<RecognizerResult>(),
            new NlpArtifacts("en"));

        Assert.Same(results, enhanced);
    }

    [Fact]
    public void ToDictionarySurfacesRecognizerMetadata()
    {
        var recognizer = new TestRecognizer();

        var description = recognizer.ToDictionary();

        Assert.Equal("TestRecognizer", description[nameof(EntityRecognizer.Name)]);
        Assert.Equal("en", description[nameof(EntityRecognizer.SupportedLanguage)]);
        Assert.Contains("PERSON", (IEnumerable<string>)description[nameof(EntityRecognizer.SupportedEntities)]!);
    }

    private sealed class TestRecognizer : EntityRecognizer
    {
        private bool _shouldLoad = true;

        public TestRecognizer()
            : base(_personEntities, supportedLanguage: "en")
        {
        }

        public int LoadCount { get; private set; }

        protected override void OnLoad()
        {
            if (!_shouldLoad)
            {
                throw new InvalidOperationException("OnLoad should not run twice.");
            }

            _shouldLoad = false;
            LoadCount++;
        }

        protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
            string text,
            IReadOnlyCollection<string> entities,
            NlpArtifacts artifacts)
        {
            if (!entities.Contains("PERSON", StringComparer.Ordinal))
            {
                return Array.Empty<RecognizerResult>();
            }

            return new[]
            {
                new RecognizerResult("PERSON", new TextSpan(0, text.Length), 0.5),
            };
        }
    }
}
