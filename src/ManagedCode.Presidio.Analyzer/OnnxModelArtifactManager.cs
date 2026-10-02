using System.Security.Cryptography;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Ensures that the ONNX model artifacts required by the NLP engine are present on disk.
/// Verifies that locally configured ONNX model artifacts exist and match optional checksums.
/// </summary>
internal static class OnnxModelArtifactManager
{
    public static void EnsureModelArtifacts(OnnxNlpEngineOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureArtifact(options.ModelPath, options.ModelChecksum);
        EnsureArtifact(options.VocabularyPath, options.VocabularyChecksum);
        EnsureArtifact(options.ConfigurationPath, options.ConfigurationChecksum);
    }

    private static void EnsureArtifact(
        string path,
        string? expectedChecksum)
    {
        if (File.Exists(path))
        {
            if (string.IsNullOrWhiteSpace(expectedChecksum) || VerifyChecksum(path, expectedChecksum))
            {
                return;
            }

            throw new InvalidDataException($"Checksum verification failed for local ONNX artifact '{path}'.");
        }
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Missing local ONNX artifact at '{path}'. Configure an explicit local model, vocabulary, and model configuration.", path);
        }
    }

    private static bool VerifyChecksum(string path, string expectedChecksum)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        var actual = Convert.ToHexString(hash);
        return actual.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }
}
