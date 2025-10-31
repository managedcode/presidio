using System.Security.Cryptography;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Ensures that the ONNX model artifacts required by the NLP engine are present on disk.
/// Downloads the canonical HuggingFace assets when they are missing or fail integrity checks.
/// </summary>
internal static class OnnxModelArtifactManager
{
    private const int DefaultBufferSize = 1 << 20; // 1 MiB

    public static void EnsureModelArtifacts(OnnxNlpEngineOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        using var httpClient = CreateHttpClient();

        EnsureArtifact(options.ModelPath, options.ModelDownloadUri, options.ModelChecksum, httpClient, cancellationToken);
        EnsureArtifact(options.VocabularyPath, options.VocabularyDownloadUri, options.VocabularyChecksum, httpClient, cancellationToken);
        EnsureArtifact(options.ConfigurationPath, options.ConfigurationDownloadUri, options.ConfigurationChecksum, httpClient, cancellationToken);
    }

    private static void EnsureArtifact(
        string path,
        Uri? downloadUri,
        string? expectedChecksum,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        if (File.Exists(path))
        {
            if (string.IsNullOrWhiteSpace(expectedChecksum) || VerifyChecksum(path, expectedChecksum))
            {
                return;
            }

            File.Delete(path);
        }

        if (downloadUri is null)
        {
            throw new FileNotFoundException($"Missing required ONNX artifact at '{path}'. Provide a local file or configure a download URI.", path);
        }

        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Unable to determine directory for artifact path '{path}'.");
        }

        Directory.CreateDirectory(directory);

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            DownloadAsync(downloadUri, tempFile, httpClient, cancellationToken).GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(expectedChecksum) && !VerifyChecksum(tempFile, expectedChecksum))
            {
                throw new InvalidDataException($"Checksum verification failed for downloaded artifact '{downloadUri}'.");
            }

            File.Move(tempFile, path, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            throw;
        }
    }

    private static async Task DownloadAsync(
        Uri uri,
        string destinationPath,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var inputStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var outputStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            DefaultBufferSize,
            useAsync: true);

        await inputStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
    }

    private static bool VerifyChecksum(string path, string expectedChecksum)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        var actual = Convert.ToHexString(hash);
        return actual.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ManagedCodePresidio/0.1 (+https://github.com/ManagedCode/presidio)");
        return client;
    }
}
