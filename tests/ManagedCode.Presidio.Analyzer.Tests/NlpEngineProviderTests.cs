using System.Text;
using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class NlpEngineProviderTests
{
    [Fact]
    public void ProviderCreatesOnnxEngineFromEmbeddedConfiguration()
    {
        var provider = new NlpEngineProvider();

        var exception = Should.Throw<FileNotFoundException>(provider.CreateEngine);

        exception.Message.ShouldContain("Missing local ONNX artifact");
    }

    [Fact]
    public void ProviderRespectsCustomConfigurationFile()
    {
        var configuration = """
nlp_engine_name: onnx
models:
  - lang_code: en
    model_path: models/ner/bert-base/model.onnx
    vocabulary_path: models/ner/bert-base/vocab.txt
    configuration_path: models/ner/bert-base/config.json
    max_sequence_length: 128
""";

        var tempFile = Path.Combine(Path.GetTempPath(), $"nlp-config-{Guid.NewGuid():N}.yaml");
        File.WriteAllText(tempFile, configuration, Encoding.UTF8);

        try
        {
            var provider = new NlpEngineProvider(configurationPath: tempFile);
            var exception = Should.Throw<FileNotFoundException>(provider.CreateEngine);

            exception.Message.ShouldContain(Path.Combine(Path.GetDirectoryName(tempFile)!, "models", "ner", "bert-base", "model.onnx"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
