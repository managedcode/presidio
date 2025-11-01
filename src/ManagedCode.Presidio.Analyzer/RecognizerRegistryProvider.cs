namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Provides configured <see cref="RecognizerRegistry"/> instances backed by YAML configuration.
/// </summary>
public sealed class RecognizerRegistryProvider(
    string? configurationPath = null,
    RecognizerRegistryConfiguration? registryConfiguration = null)
{
    private readonly string? _configurationPath = configurationPath;
    private readonly RecognizerRegistryConfiguration? _configurationOverride = registryConfiguration;

    public RecognizerRegistry CreateRecognizerRegistry(
        INlpEngine? nlpEngine = null,
        IReadOnlyCollection<string>? supportedLanguages = null)
    {
        var configuration = _configurationOverride ?? RecognizerRegistryConfigurationLoader.Load(_configurationPath);
        var registry = new RecognizerRegistry();
        registry.LoadPredefinedRecognizers(configuration, nlpEngine, supportedLanguages);
        return registry;
    }
}

