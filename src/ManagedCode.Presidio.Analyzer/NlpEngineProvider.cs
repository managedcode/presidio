using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Creates NLP engine instances based on configuration, mirroring Presidio's Python provider.
/// </summary>
public sealed class NlpEngineProvider
{
    private const string DefaultConfigurationResource = "ManagedCode.Presidio.Analyzer.conf.nlp.default.yaml";

    private readonly IReadOnlyDictionary<string, INlpEngineFactory> _engineFactories;
    private readonly NlpEngineConfiguration _configuration;

    public NlpEngineProvider(
        IEnumerable<INlpEngineFactory>? engineFactories = null,
        string? configurationPath = null,
        NlpEngineConfiguration? configuration = null)
    {
        if (configuration is not null && !string.IsNullOrWhiteSpace(configurationPath))
        {
            throw new ArgumentException("Provide either a configuration object or a configuration path, not both.");
        }

        _engineFactories = (engineFactories ?? new INlpEngineFactory[] { new OnnxNlpEngineFactory() })
            .Where(factory => factory is not null && factory.IsAvailable)
            .ToDictionary(factory => factory.Name, StringComparer.OrdinalIgnoreCase);

        if (_engineFactories.Count == 0)
        {
            throw new InvalidOperationException("No NLP engine factories are available.");
        }

        if (configuration is not null)
        {
            var baseDirectory = configuration.BaseDirectory ?? AppContext.BaseDirectory;
            _configuration = configuration.WithBaseDirectory(baseDirectory);
        }
        else if (!string.IsNullOrWhiteSpace(configurationPath))
        {
            _configuration = LoadConfigurationFromFile(configurationPath);
        }
        else
        {
            _configuration = LoadConfigurationFromEmbeddedResource();
        }

        NlpEngineConfigurationValidator.Validate(_configuration, _engineFactories.Keys.ToArray());
    }

    public INlpEngine CreateEngine()
    {
        if (!_engineFactories.TryGetValue(_configuration.EngineName, out var factory))
        {
            throw new InvalidOperationException($"NLP engine '{_configuration.EngineName}' is not registered.");
        }

        var engine = factory.Create(_configuration);
        engine.Load();
        return engine;
    }

    private static NlpEngineConfiguration LoadConfigurationFromFile(string configurationPath)
    {
        if (!File.Exists(configurationPath))
        {
            throw new FileNotFoundException($"NLP configuration file '{configurationPath}' was not found.", configurationPath);
        }

        using var reader = File.OpenText(configurationPath);
        var configuration = BuildDeserializer().Deserialize<NlpEngineConfiguration>(reader)
            ?? throw new InvalidOperationException("Unable to deserialize NLP configuration.");

        var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(configurationPath)) ?? AppContext.BaseDirectory;
        return configuration.WithBaseDirectory(baseDirectory);
    }

    private static NlpEngineConfiguration LoadConfigurationFromEmbeddedResource()
    {
        var assembly = typeof(NlpEngineProvider).Assembly;
        using var stream = assembly.GetManifestResourceStream(DefaultConfigurationResource)
            ?? throw new InvalidOperationException($"Embedded NLP configuration '{DefaultConfigurationResource}' could not be located.");

        using var reader = new StreamReader(stream);
        var configuration = BuildDeserializer().Deserialize<NlpEngineConfiguration>(reader)
            ?? throw new InvalidOperationException("Unable to deserialize embedded NLP configuration.");

        return configuration.WithBaseDirectory(AppContext.BaseDirectory);
    }

    private static IDeserializer BuildDeserializer() =>
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
}
