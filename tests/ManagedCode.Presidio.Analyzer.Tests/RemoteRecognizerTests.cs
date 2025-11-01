using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class RemoteRecognizerTests
{
    [Fact]
    public void AnalyzeDelegatesToRemoteImplementation()
    {
        var recognizer = new TestRemoteRecognizer();
        var artifacts = new NlpArtifacts("en");

        var results = recognizer.Analyze("data", new[] { "TEST" }, artifacts);

        var result = results.ShouldHaveSingleItem();
        result.EntityType.ShouldBe("TEST");
        result.Span.ShouldBe(new TextSpan(0, 4));
        recognizer.Called.ShouldBeTrue();
    }

    private sealed class TestRemoteRecognizer : RemoteRecognizer
    {
        public bool Called { get; private set; }

        public TestRemoteRecognizer()
            : base(new[] { "TEST" }, supportedLanguage: "en")
        {
        }

        protected override IReadOnlyCollection<RecognizerResult> AnalyzeRemote(
            string text,
            IReadOnlyCollection<string> entities,
            NlpArtifacts artifacts)
        {
            Called = true;
            return new[]
            {
                new RecognizerResult("TEST", new TextSpan(0, text?.Length ?? 0), EntityRecognizer.MaxScore),
            };
        }
    }
}
