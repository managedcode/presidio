using System.Collections.ObjectModel;
using YamlDotNet.Serialization;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Represents the configuration needed to bootstrap an NLP engine.
/// Mirrors the semantics of Presidio's Python configuration.
/// </summary>
public sealed class NlpEngineConfiguration
{
    [YamlMember(Alias = "nlp_engine_name")]
    public string EngineName { get; init; } = string.Empty;

    [YamlMember(Alias = "models")]
    public IReadOnlyList<NlpModelConfiguration> Models { get; init; } = Array.Empty<NlpModelConfiguration>();

    [YamlMember(Alias = "ner_model_configuration")]
    public NerModelConfiguration? NerModelConfiguration { get; init; }

    [YamlIgnore]
    public string? BaseDirectory { get; init; }

    public NlpEngineConfiguration WithBaseDirectory(string? baseDirectory) =>
        new()
        {
            EngineName = EngineName,
            Models = Models,
            NerModelConfiguration = NerModelConfiguration,
            BaseDirectory = baseDirectory,
        };
}

/// <summary>
/// Describes a language-specific model required by an NLP engine.
/// </summary>
public sealed class NlpModelConfiguration
{
    [YamlMember(Alias = "lang_code")]
    public string LanguageCode { get; init; } = string.Empty;

    [YamlMember(Alias = "model_name")]
    public string? ModelName { get; init; }

    [YamlMember(Alias = "model_path")]
    public string? ModelPath { get; init; }

    [YamlMember(Alias = "vocabulary_path")]
    public string? VocabularyPath { get; init; }

    [YamlMember(Alias = "configuration_path")]
    public string? ConfigurationPath { get; init; }

    [YamlMember(Alias = "metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    [YamlMember(Alias = "max_sequence_length")]
    public int? MaxSequenceLength { get; init; }
}

/// <summary>
/// Optional configuration for named entity mapping and post-processing.
/// </summary>
public sealed class NerModelConfiguration
{
    [YamlMember(Alias = "model_to_presidio_entity_mapping")]
    public IReadOnlyDictionary<string, string> ModelToPresidioEntityMapping { get; init; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    [YamlMember(Alias = "low_confidence_score_multiplier")]
    public double? LowConfidenceScoreMultiplier { get; init; }

    [YamlMember(Alias = "low_score_entity_names")]
    public IReadOnlyList<string> LowScoreEntityNames { get; init; } = Array.Empty<string>();

    [YamlMember(Alias = "labels_to_ignore")]
    public IReadOnlyList<string> LabelsToIgnore { get; init; } = Array.Empty<string>();
}

internal static class NlpEngineConfigurationValidator
{
    public static void Validate(
        NlpEngineConfiguration configuration,
        IReadOnlyCollection<string> availableEngineNames)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (string.IsNullOrWhiteSpace(configuration.EngineName))
        {
            throw new InvalidOperationException("NLP configuration must specify 'nlp_engine_name'.");
        }

        if (!availableEngineNames.Contains(configuration.EngineName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"NLP engine '{configuration.EngineName}' is not registered or not available.");
        }

        if (configuration.Models is null || configuration.Models.Count == 0)
        {
            throw new InvalidOperationException("NLP configuration must include at least one model entry.");
        }

        foreach (var model in configuration.Models)
        {
            if (string.IsNullOrWhiteSpace(model.LanguageCode))
            {
                throw new InvalidOperationException("Each NLP model definition must include 'lang_code'.");
            }
        }
    }
}
