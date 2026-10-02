using System.Text.RegularExpressions;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class RecognizerRegistryCoverageTests
{
    [Fact]
    public void GetRecognizersFiltersAllFieldsByRequestedLanguage()
    {
        var registry = new RecognizerRegistry(
            [new EmailRecognizer(), new EsNifRecognizer()],
            ["en", "es"]);

        Assert.Equal(["EMAIL_ADDRESS"], registry.GetRecognizers("en", allFields: true).SelectMany(recognizer => recognizer.SupportedEntities));
        Assert.Equal(["ES_NIF"], registry.GetRecognizers("es", allFields: true).SelectMany(recognizer => recognizer.SupportedEntities));
        Assert.Throws<InvalidOperationException>(() => registry.GetRecognizers("fr", allFields: true));
        Assert.Equal(["ES_NIF"], registry.GetSupportedEntities(["es"]));
    }

    [Fact]
    public void GetRecognizersRejectsRequestsWithAnyUnsupportedEntity()
    {
        var registry = new RecognizerRegistry([new EmailRecognizer()], ["en"]);

        Assert.Throws<InvalidOperationException>(() => registry.GetRecognizers(
            "en",
            ["EMAIL_ADDRESS", "PERSON"]));
    }

    [Fact]
    public void EnabledUnknownRecognizerConfigurationFailsInsteadOfReducingCoverageSilently()
    {
        var configuration = CreateConfiguration(new RecognizerDefinition(
            "MissingRecognizer",
            "predefined",
            enabled: true,
            [new RecognizerLanguageConfiguration("en", Array.Empty<string>())],
            new Dictionary<string, object?>()));
        var registry = new RecognizerRegistry();

        Assert.Throws<InvalidOperationException>(() => registry.LoadPredefinedRecognizers(configuration));
    }

    [Fact]
    public void EnabledUnsupportedRecognizerTypeFailsClosed()
    {
        var configuration = CreateConfiguration(new RecognizerDefinition(
            "ExternalRecognizer",
            "custom",
            enabled: true,
            [new RecognizerLanguageConfiguration("en", Array.Empty<string>())],
            new Dictionary<string, object?>()));
        var registry = new RecognizerRegistry();

        Assert.Throws<NotSupportedException>(() => registry.LoadPredefinedRecognizers(configuration));
    }

    private static RecognizerRegistryConfiguration CreateConfiguration(RecognizerDefinition definition) => new(
        ["en"],
        RegexOptions.None,
        [definition],
        source: "test configuration");
}
