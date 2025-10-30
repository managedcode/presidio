using System;
using Xunit;

namespace ManagedCode.Presidio.Core.Tests;

public sealed class AnalysisExplanationTests
{
    [Fact]
    public void SetSupportiveContextWordStoresValue()
    {
        var explanation = new AnalysisExplanation("regex", 0.25);

        explanation.SetSupportiveContextWord("account");

        Assert.Equal("account", explanation.SupportiveContextWord);
    }

    [Fact]
    public void AppendTextualExplanationAccumulatesLines()
    {
        var explanation = new AnalysisExplanation("regex", 0.25);

        explanation.AppendTextualExplanationLine("first");
        explanation.AppendTextualExplanationLine("second");

        Assert.Equal($"first{Environment.NewLine}second", explanation.TextualExplanation);
    }

    [Fact]
    public void ToDictionaryExposesComputedValues()
    {
        var explanation = new AnalysisExplanation("regex", 0.2, patternName: "digits");
        explanation.SetImprovedScore(0.6);
        explanation.SetSupportiveContextWord("phone");

        var snapshot = explanation.ToDictionary();

        Assert.Equal("regex", snapshot[nameof(AnalysisExplanation.Recognizer)]);
        Assert.Equal("digits", snapshot[nameof(AnalysisExplanation.PatternName)]);
        Assert.Equal(0.6, snapshot[nameof(AnalysisExplanation.Score)]);
        Assert.Equal("phone", snapshot[nameof(AnalysisExplanation.SupportiveContextWord)]);
        Assert.Equal(0.4, (double)snapshot[nameof(AnalysisExplanation.ScoreContextImprovement)]!, 5);
    }
}
