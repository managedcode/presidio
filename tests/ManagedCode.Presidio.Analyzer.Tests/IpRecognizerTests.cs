using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class IpRecognizerTests
{
    [Theory]
    [InlineData("microsoft.com 192.168.0.1", "192.168.0.1", 0.6)]
    [InlineData("my ip: 684D:1111:222:3333:4444:5555:6:77", "684D:1111:222:3333:4444:5555:6:77", 0.6)]
    [InlineData("2345:0425:2CA1:0000:0000:0567:5673:23b5", "2345:0425:2CA1:0000:0000:0567:5673:23b5", 0.6)]
    [InlineData("2345:0425:2CA1::0567:5673:23b5", "2345:0425:2CA1::0567:5673:23b5", 0.6)]
    [InlineData("Use local ipv6 ::", "::", 0.1)]
    public void RecognizerDetectsValidIpAddresses(string text, string expectedMatch, double expectedScore)
    {
        var recognizer = new IpRecognizer();
        var results = recognizer.Analyze(text, new[] { "IP_ADDRESS" }, new NlpArtifacts("en")).ToList();

        var match = Assert.Single(results);
        Assert.Equal("IP_ADDRESS", match.EntityType);
        Assert.Equal(expectedScore, match.Score, 3);

        var segment = text[match.Start..match.End];
        Assert.Equal(expectedMatch, segment);
    }

    [Theory]
    [InlineData("my ip: 192.168.0")]
    [InlineData("684D:1111:222:3333:4444:5555:77")]
    [InlineData("invalid ip 999.999.999.999")]
    public void RecognizerFiltersInvalidIpAddresses(string text)
    {
        var recognizer = new IpRecognizer();
        var results = recognizer.Analyze(text, new[] { "IP_ADDRESS" }, new NlpArtifacts("en"));

        Assert.Empty(results);
    }
}
