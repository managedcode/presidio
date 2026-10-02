using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class OnnxLocalArtifactsTests
{
    [Fact]
    public void OnnxEngineFailsFastWhenExplicitLocalArtifactsAreMissing()
    {
        var artifactDirectory = Path.Combine(Path.GetTempPath(), "managedcode-presidio-missing", Guid.NewGuid().ToString("N"));
        var options = new OnnxNlpEngineOptions
        {
            ModelPath = Path.Combine(artifactDirectory, "model.onnx"),
            VocabularyPath = Path.Combine(artifactDirectory, "vocab.txt"),
            ConfigurationPath = Path.Combine(artifactDirectory, "config.json"),
        };

        var exception = Should.Throw<FileNotFoundException>(() => new OnnxNlpEngine(options));

        exception.Message.ShouldContain("Missing local ONNX artifact");
    }
}
