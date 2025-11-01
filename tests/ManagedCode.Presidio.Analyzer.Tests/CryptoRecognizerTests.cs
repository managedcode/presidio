using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class CryptoRecognizerTests
{
    [Theory]
    [InlineData("16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", "16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ")]
    [InlineData("3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy", "3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy")]
    [InlineData("bc1qar0srrr7xfkvy5l643lydnw9re59gtzzwf5mdq", "bc1qar0srrr7xfkvy5l643lydnw9re59gtzzwf5mdq")]
    [InlineData("bc1p5d7rjq7g6rdk2yhzks9smlaqtedr4dekq08ge8ztwac72sfr9rusxg3297", "bc1p5d7rjq7g6rdk2yhzks9smlaqtedr4dekq08ge8ztwac72sfr9rusxg3297")]
    [InlineData("SegWit: bc1qar0srrr7xfkvy5l643lydnw9re59gtzzwf5mdq", "bc1qar0srrr7xfkvy5l643lydnw9re59gtzzwf5mdq")]
    [InlineData("my wallet address is: 16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", "16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ")]
    public void AnalyzerDetectsValidWallets(string text, string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        bool Matches(RecognizerResult result) =>
            result.EntityType == "CRYPTO" &&
            Slice(text, result).Equals(expected, StringComparison.Ordinal);

        var recognizer = new CryptoRecognizer();
        var directResults = recognizer.Analyze(text, new[] { "CRYPTO" }, new NlpArtifacts("en")).ToList();

        directResults.ShouldContain(Matches);
        var directMatch = directResults.First(Matches);
        directMatch.Score.ShouldBe(EntityRecognizer.MaxScore);

        using var engine = new AnalyzerEngine();
        engine.GetRecognizers("en").OfType<CryptoRecognizer>().ShouldNotBeEmpty();
        var engineResults = engine.Analyze(text, "en", new[] { "CRYPTO" }).ToList();

        engineResults.ShouldContain(Matches);
        var engineMatch = engineResults.First(Matches);
        engineMatch.Score.ShouldBe(EntityRecognizer.MaxScore);
    }

    [Theory]
    [InlineData("Invalid wallet 1BoatSLRHtKNngkdXEeobR76b53LETtpyU")]
    [InlineData("Invalid wallet bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kygt081")]
    [InlineData("Invalid SegWit bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kygt080")]
    [InlineData("")]
    [InlineData("8f953371d3e85eddb89b05ed6b9e680791055315c73e1025ab5dba7bb2aee189")]
    [InlineData("my wallet address is: 16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ2")]
    public void AnalyzerFiltersInvalidWallets(string text)
    {
        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CRYPTO" }).ToList();

        results.ShouldBeEmpty();
    }

    [Fact]
    public void AnalyzerDetectsMultipleWallets()
    {
        const string text = "16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ 3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CRYPTO" }).ToList();
        results.Count.ShouldBe(2);
        results.ShouldContain(r => Slice(text, r).Equals("16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", StringComparison.Ordinal));
        results.ShouldContain(r => Slice(text, r).Equals("3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy", StringComparison.Ordinal));

        var recognizer = new CryptoRecognizer();
        var directResults = recognizer.Analyze(text, new[] { "CRYPTO" }, new NlpArtifacts("en")).ToList();
        directResults.Count.ShouldBe(2);
        directResults.ShouldContain(r => Slice(text, r).Equals("16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", StringComparison.Ordinal));
        directResults.ShouldContain(r => Slice(text, r).Equals("3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy", StringComparison.Ordinal));
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
