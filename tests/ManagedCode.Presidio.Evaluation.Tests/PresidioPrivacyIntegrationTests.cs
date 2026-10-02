using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Evaluation.Tests;

public sealed class PresidioPrivacyIntegrationTests
{
    private static readonly PresidioPiiPolicy Policy = new("en", ["EMAIL_ADDRESS", "US_SSN"], 0.5);

    [Fact]
    public async Task EvaluatorReportsSensitiveIdentifiersWithoutEchoingTheirValues()
    {
        using var analyzer = CreateAnalyzer();
        var evaluator = new PresidioPrivacyEvaluator(analyzer, Policy);
        const string response = "Contact alice@example.org about SSN 078-05-1123.";

        var result = await evaluator.EvaluateAsync(
            [new ChatMessage(ChatRole.User, "Who should I contact?")],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));

        var metric = result.Get<NumericMetric>(PresidioPrivacyEvaluator.PiiFindingsMetricName);
        metric.Value.ShouldBe(2);
        metric.Interpretation!.Failed.ShouldBeTrue();
        metric.Reason.ShouldNotBeNull();
        metric.Reason.ShouldNotContain("alice@example.org");
        metric.Reason.ShouldNotContain("078-05-1123");
    }

    [Fact]
    public async Task EvaluatorPassesResponseWithoutSelectedPii()
    {
        using var analyzer = CreateAnalyzer();
        var evaluator = new PresidioPrivacyEvaluator(analyzer, Policy);

        var result = await evaluator.EvaluateAsync(
            [new ChatMessage(ChatRole.User, "What is the weather?")],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "It is sunny today.")));

        var metric = result.Get<NumericMetric>(PresidioPrivacyEvaluator.PiiFindingsMetricName);
        metric.Value.ShouldBe(0);
        metric.Interpretation!.Failed.ShouldBeFalse();
    }

    [Fact]
    public void SanitizerUsesRealAnalyzerAndAnonymizerAndKeepsSourceOffsets()
    {
        using var analyzer = CreateAnalyzer();
        var sanitizer = new PresidioEvidenceSanitizer(analyzer, new AnonymizerEngine());
        const string evidence = "Contact alice@example.org; identifier 078-05-1123.";

        var result = sanitizer.Sanitize(evidence, Policy, PresidioEvidenceRedactionMode.ReplaceWithEntity);

        result.Text.ShouldBe("Contact <EMAIL_ADDRESS>; identifier <US_SSN>.");
        result.Text.ShouldNotContain("alice@example.org");
        result.Text.ShouldNotContain("078-05-1123");
        result.Findings.Select(f => evidence[f.Start..f.End]).ShouldBe(["alice@example.org", "078-05-1123"]);
        result.Findings.Select(f => f.EntityType).ShouldBe(["EMAIL_ADDRESS", "US_SSN"]);
    }

    [Fact]
    public void SanitizerRejectsUnknownRedactionModeInsteadOfReturningEvidenceUnchanged()
    {
        using var analyzer = CreateAnalyzer();
        var sanitizer = new PresidioEvidenceSanitizer(analyzer, new AnonymizerEngine());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            sanitizer.Sanitize("alice@example.org", Policy, (PresidioEvidenceRedactionMode)int.MaxValue));
    }

    [Fact]
    public void EvaluatorRejectsMixedSupportedAndUnsupportedEntityCoverage()
    {
        using var analyzer = new AnalyzerEngine();
        var unsupportedPolicy = new PresidioPiiPolicy("en", ["EMAIL_ADDRESS", "PERSON"], 0.5);

        Should.Throw<ArgumentException>(() => new PresidioPrivacyEvaluator(analyzer, unsupportedPolicy));
        Should.Throw<InvalidOperationException>(() => analyzer.Analyze(
            "alice@example.org",
            "en",
            ["EMAIL_ADDRESS", "PERSON"]));
    }

    [Fact]
    public void DefaultAnalyzerDoesNotClaimNlpOnlyPersonCoverageWithoutLocalNlp()
    {
        using var analyzer = new AnalyzerEngine();

        analyzer.GetSupportedEntities("en").ShouldNotContain("PERSON");
        Should.Throw<ArgumentException>(() => new PresidioPrivacyEvaluator(
            analyzer,
            new PresidioPiiPolicy("en", ["PERSON"], 0.5)));
    }

    private static AnalyzerEngine CreateAnalyzer()
    {
        return new AnalyzerEngine();
    }
}
