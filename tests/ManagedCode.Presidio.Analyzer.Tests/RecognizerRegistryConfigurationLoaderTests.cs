using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class RecognizerRegistryConfigurationLoaderTests
{
    [Fact]
    public void Load_ReturnsDefaultConfiguration()
    {
        var configuration = RecognizerRegistryConfigurationLoader.Load();

        configuration.ShouldNotBeNull();
        configuration.SupportedLanguages.ShouldContain("en");
        configuration.GlobalRegexOptions.ShouldBe(
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

        configuration.Recognizers.ShouldContain(definition => definition.Name == nameof(CreditCardRecognizer));
        configuration.Recognizers.ShouldContain(definition => definition.Name == nameof(UsPassportRecognizer));
        configuration.Recognizers.ShouldContain(definition => definition.Name == nameof(UsItinRecognizer));
    }
}
