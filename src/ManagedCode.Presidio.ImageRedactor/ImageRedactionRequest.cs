using System.Collections.ObjectModel;

namespace ManagedCode.Presidio.ImageRedactor;

/// <summary>
/// Represents a request to anonymize an image with given redaction regions.
/// </summary>
public sealed class ImageRedactionRequest
{
    private readonly List<BoundingBox> _regions = new();

    public ImageRedactionRequest(ReadOnlyMemory<byte> imageBytes)
    {
        if (imageBytes.IsEmpty)
        {
            throw new ArgumentException("Image data must not be empty.", nameof(imageBytes));
        }

        ImageBytes = imageBytes;
    }

    public ReadOnlyMemory<byte> ImageBytes { get; }

    public IReadOnlyList<BoundingBox> Regions => new ReadOnlyCollection<BoundingBox>(_regions);

    public void AddRegion(BoundingBox region)
    {
        if (_regions.Contains(region))
        {
            return;
        }

        _regions.Add(region);
    }
}
