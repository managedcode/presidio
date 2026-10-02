namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Factory for creating <see cref="OnnxNlpEngine" /> instances from configuration.
/// </summary>
public sealed class OnnxNlpEngineFactory : INlpEngineFactory
{
    public string Name => "onnx";

    public bool IsAvailable => true;

    public INlpEngine Create(NlpEngineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Models.Count == 0)
        {
            throw new InvalidOperationException("ONNX NLP engine requires at least one model configuration.");
        }

        var model = configuration.Models[0];
        var baseDirectory = configuration.BaseDirectory;
        var defaults = new OnnxNlpEngineOptions();

        var options = new OnnxNlpEngineOptions
        {
            Language = string.IsNullOrWhiteSpace(model.LanguageCode) ? defaults.Language : model.LanguageCode,
            ModelPath = ResolvePath(model.ModelPath, baseDirectory, defaults.ModelPath),
            VocabularyPath = ResolvePath(model.VocabularyPath, baseDirectory, defaults.VocabularyPath),
            ConfigurationPath = ResolvePath(model.ConfigurationPath, baseDirectory, defaults.ConfigurationPath),
            MaxSequenceLength = model.MaxSequenceLength ?? defaults.MaxSequenceLength,
            OutputName = ResolveRequiredMetadata(model, "output_name", defaults.OutputName),
            InputIdsName = ResolveRequiredMetadata(model, "input_ids_name", defaults.InputIdsName),
            AttentionMaskName = ResolveRequiredMetadata(model, "attention_mask_name", defaults.AttentionMaskName),
            TokenTypeIdsName = ResolveRequiredMetadata(model, "token_type_ids_name", defaults.TokenTypeIdsName),
            ModelChecksum = ResolveOptionalMetadata(model, "model_checksum", defaults.ModelChecksum),
            VocabularyChecksum = ResolveOptionalMetadata(model, "vocabulary_checksum", defaults.VocabularyChecksum),
            ConfigurationChecksum = ResolveOptionalMetadata(model, "configuration_checksum", defaults.ConfigurationChecksum),
        };

        return new OnnxNlpEngine(options);
    }

    private static string ResolvePath(string? candidate, string? baseDirectory, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return defaultValue;
        }

        if (Path.IsPathFullyQualified(candidate))
        {
            return candidate;
        }

        if (!string.IsNullOrWhiteSpace(baseDirectory))
        {
            return Path.GetFullPath(Path.Combine(baseDirectory, candidate));
        }

        return Path.GetFullPath(candidate);
    }

    private static string ResolveRequiredMetadata(NlpModelConfiguration model, string key, string defaultValue)
    {
        var candidate = ResolveOptionalMetadata(model, key, defaultValue);
        return string.IsNullOrWhiteSpace(candidate) ? defaultValue : candidate;
    }

    private static string? ResolveOptionalMetadata(NlpModelConfiguration model, string key, string? defaultValue)
    {
        if (model.Metadata is not null && model.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return defaultValue;
    }

}
