using ManagedCode.Presidio.Core;
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

        var directRecognizer = new CryptoRecognizer();
        var directResults = directRecognizer.Analyze(text, new[] { "CRYPTO" }, new NlpArtifacts("en")).ToList();

        bool Matches(RecognizerResult result) =>
            result.EntityType == "CRYPTO" &&
            Slice(text, result).Equals(expected, StringComparison.Ordinal);

        Assert.Contains(directResults, Matches);
        var directMatch = directResults.First(Matches);
        Assert.Equal(EntityRecognizer.MaxScore, directMatch.Score);

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CRYPTO" }).ToList();
        Assert.True(engine.GetRecognizers("en").OfType<CryptoRecognizer>().Any(), "CryptoRecognizer is not registered for 'en'.");
        Assert.Contains(results, Matches);
        var engineMatch = results.First(Matches);
        Assert.Equal(EntityRecognizer.MaxScore, engineMatch.Score);
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

        Assert.Empty(results);
    }

    [Fact]
    public void AnalyzerDetectsMultipleWallets()
    {
        const string text = "16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ 3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy";

        using var engine = new AnalyzerEngine();
        var results = engine.Analyze(text, "en", new[] { "CRYPTO" }).ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => Slice(text, r).Equals("16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", StringComparison.Ordinal));
        Assert.Contains(results, r => Slice(text, r).Equals("3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy", StringComparison.Ordinal));

        var recognizer = new CryptoRecognizer();
        var directResults = recognizer.Analyze(text, new[] { "CRYPTO" }, new NlpArtifacts("en")).ToList();
        Assert.Equal(2, directResults.Count);
        Assert.Contains(directResults, r => Slice(text, r).Equals("16Yeky6GMjeNkAiNcBY7ZhrLoMSgg1BoyZ", StringComparison.Ordinal));
        Assert.Contains(directResults, r => Slice(text, r).Equals("3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy", StringComparison.Ordinal));
    }

    private static string Slice(string text, RecognizerResult result) => text[result.Start..result.End];
}
