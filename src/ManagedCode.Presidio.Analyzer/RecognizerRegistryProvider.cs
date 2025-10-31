namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Provides configured <see cref="RecognizerRegistry"/> instances.
/// </summary>
public sealed class RecognizerRegistryProvider(RecognizerRegistryConfiguration? registryConfiguration = null)
{
    private readonly RecognizerRegistryConfiguration _configuration = registryConfiguration ?? new RecognizerRegistryConfiguration();

    public RecognizerRegistry CreateRecognizerRegistry()
    {
        return new RecognizerRegistry(supportedLanguages: _configuration.SupportedLanguages);
    }
}

public sealed class RecognizerRegistryConfiguration(
    IReadOnlyCollection<string>? supportedLanguages = null)
{
    public IReadOnlyCollection<string> SupportedLanguages { get; } = supportedLanguages ?? Array.Empty<string>();
}
