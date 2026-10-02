using System.Text.Json;
using System.Text.Json.Nodes;

namespace ManagedCode.Presidio.Structured;

/// <summary>Reads JSON documents using the platform JSON parser.</summary>
public sealed class JsonDataReader
{
    public JsonNode Read(Stream stream, JsonDocumentOptions options = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return JsonNode.Parse(stream, documentOptions: options)
            ?? throw new JsonException("The JSON document must have a root value.");
    }

    public JsonNode Read(string path, JsonDocumentOptions options = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        return Read(stream, options);
    }
}
