namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Configuration for the ONNX-backed NLP engine.
/// </summary>
public sealed class OnnxNlpEngineOptions
{
    private const string DefaultModelRelativePath = "models/ner/bert-base/model.onnx";
    private const string DefaultVocabRelativePath = "models/ner/bert-base/vocab.txt";
    private const string DefaultConfigRelativePath = "models/ner/bert-base/config.json";
    private static readonly Uri DefaultModelDownloadUri = new("https://huggingface.co/dslim/bert-base-NER/resolve/main/onnx/model.onnx");
    private static readonly Uri DefaultVocabDownloadUri = new("https://huggingface.co/dslim/bert-base-NER/resolve/main/vocab.txt");
    private static readonly Uri DefaultConfigDownloadUri = new("https://huggingface.co/dslim/bert-base-NER/resolve/main/config.json");
    private const string DefaultModelChecksum = "963039B81EEC5B33E23D84826CCDF1E8F8ADA776F320E692113034CFAE384617";
    private const string DefaultVocabChecksum = "EEAA9875B23B04B4C54EF759D03DB9D1BA1554838F8FB26C5D96FA551DF93D02";
    private const string DefaultConfigChecksum = "A5DC77A0D60DADF5645103017F3C7B8ECE4EF1B450BC1EA68E17DA1D3AE7E515";

    public string ModelPath { get; init; } = ResolvePath(DefaultModelRelativePath);

    public string VocabularyPath { get; init; } = ResolvePath(DefaultVocabRelativePath);

    public string ConfigurationPath { get; init; } = ResolvePath(DefaultConfigRelativePath);

    public Uri? ModelDownloadUri { get; init; } = DefaultModelDownloadUri;

    public string? ModelChecksum { get; init; } = DefaultModelChecksum;

    public Uri? VocabularyDownloadUri { get; init; } = DefaultVocabDownloadUri;

    public string? VocabularyChecksum { get; init; } = DefaultVocabChecksum;

    public Uri? ConfigurationDownloadUri { get; init; } = DefaultConfigDownloadUri;

    public string? ConfigurationChecksum { get; init; } = DefaultConfigChecksum;

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
