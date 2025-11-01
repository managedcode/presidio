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

        using var engine = provider.CreateEngine();

        engine.IsLoaded.ShouldBeTrue();
        engine.GetSupportedLanguages().ShouldContain("en");
        var artifacts = engine.ProcessText("Hello world", "en");
        artifacts.Language.ShouldBe("en");
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
            using var engine = provider.CreateEngine();

            engine.IsLoaded.ShouldBeTrue();
            engine.GetSupportedLanguages().ShouldContain("en");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
