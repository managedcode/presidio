using System.Collections.ObjectModel;

namespace ManagedCode.Presidio.ImageRedactor;

/// <summary>
/// Captures the output of an image redaction operation.
/// </summary>
public sealed class ImageRedactionResult
{
    public ImageRedactionResult(ReadOnlyMemory<byte> imageBytes, IEnumerable<BoundingBox> redactedRegions)
    {
        if (imageBytes.IsEmpty)
        {
            throw new ArgumentException("Redacted image must not be empty.", nameof(imageBytes));
        }

        ImageBytes = imageBytes;
        RedactedRegions = new ReadOnlyCollection<BoundingBox>(new List<BoundingBox>(redactedRegions ?? Array.Empty<BoundingBox>()));
    }

    public ReadOnlyMemory<byte> ImageBytes { get; }

    public IReadOnlyList<BoundingBox> RedactedRegions { get; }
}
