using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InVehicleRegistrationRecognizerTests
{
    private static readonly (string Text, string Expected, double MinScore)[] ValidCases =
    {
        ("KA53ME3456", "KA53ME3456", 0.5),
        ("KA99ME3456", "KA99ME3456", 0.5),
        ("MN2412", "MN2412", 0.01),
        ("MCX1243", "MCX1243", 0.2),
        ("I15432", "I15432", 0.01),
        ("DL3CJI0001", "DL3CJI0001", EntityRecognizer.MaxScore),
        ("My Bike's registration number is OD02BA2341 with a lot of text beyond", "OD02BA2341", EntityRecognizer.MaxScore),
    };

    [Theory]
    [MemberData(nameof(GetValidCases))]
    public void RecognizerDetectsValidRegistrations(string text, string expected, double minScore)
    {
        var recognizer = new InVehicleRegistrationRecognizer();
        var result = recognizer.Analyze(text, new[] { "IN_VEHICLE_REGISTRATION" }, new NlpArtifacts("en")).ShouldHaveSingleItem();

        result.EntityType.ShouldBe("IN_VEHICLE_REGISTRATION");
        result.Score.ShouldBeGreaterThanOrEqualTo(minScore);
        text[result.Start..result.End].ShouldBe(expected);
    }

    [Theory]
    [InlineData("ABNE123456")]
    [InlineData("XYZ0000")]
    public void RecognizerFiltersInvalidRegistrations(string text)
    {
        var recognizer = new InVehicleRegistrationRecognizer();
        var results = recognizer.Analyze(text, new[] { "IN_VEHICLE_REGISTRATION" }, new NlpArtifacts("en"));

        results.ShouldBeEmpty();
    }

    public static TheoryData<string, string, double> GetValidCases()
    {
        var data = new TheoryData<string, string, double>();
        foreach (var (text, expected, minScore) in ValidCases)
        {
            data.Add(text, expected, minScore);
        }

        return data;
    }
}
