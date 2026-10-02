namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Configuration for the ONNX-backed NLP engine.
/// </summary>
public sealed class OnnxNlpEngineOptions
{
    private const string DefaultModelRelativePath = "models/ner/bert-base/model.onnx";
    private const string DefaultVocabRelativePath = "models/ner/bert-base/vocab.txt";
    private const string DefaultConfigRelativePath = "models/ner/bert-base/config.json";
    public string ModelPath { get; init; } = ResolvePath(DefaultModelRelativePath);

    public string VocabularyPath { get; init; } = ResolvePath(DefaultVocabRelativePath);

    public string ConfigurationPath { get; init; } = ResolvePath(DefaultConfigRelativePath);

    public string? ModelChecksum { get; init; }

    public string? VocabularyChecksum { get; init; }

    public string? ConfigurationChecksum { get; init; }

    public string Language { get; init; } = "en";

    public int MaxSequenceLength { get; init; } = 256;

    public string OutputName { get; init; } = "logits";

    public string InputIdsName { get; init; } = "input_ids";

    public string AttentionMaskName { get; init; } = "attention_mask";

    public string TokenTypeIdsName { get; init; } = "token_type_ids";

    private static string ResolvePath(string relativePath)
    {
        if (Path.IsPathFullyQualified(relativePath))
        {
            return relativePath;
        }

        var baseDirectory = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDirectory, relativePath));
    }
}
