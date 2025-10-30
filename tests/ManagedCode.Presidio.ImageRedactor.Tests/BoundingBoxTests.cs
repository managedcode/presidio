using Xunit;

namespace ManagedCode.Presidio.ImageRedactor.Tests;

public sealed class BoundingBoxTests
{
    [Fact]
    public void ConstructorValidatesDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundingBox(0, 0, 0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundingBox(0, 0, 10, 0));
    }

    [Fact]
    public void IntersectsReturnsTrueForOverlappingRegions()
    {
        var first = new BoundingBox(0, 0, 10, 10);
        var second = new BoundingBox(5, 5, 10, 10);

        Assert.True(first.Intersects(second));
        Assert.True(second.Intersects(first));
    }

    [Fact]
    public void IntersectsReturnsFalseForSeparatedRegions()
    {
        var first = new BoundingBox(0, 0, 5, 5);
        var second = new BoundingBox(10, 10, 5, 5);

        Assert.False(first.Intersects(second));
    }

    [Fact]
    public void EqualityComparesPositionAndSize()
    {
        var first = new BoundingBox(0, 0, 5, 5);
        var second = new BoundingBox(0, 0, 5, 5);
        var third = new BoundingBox(1, 0, 5, 5);

        Assert.Equal(first, second);
        Assert.NotEqual(first, third);
        Assert.True(first == second);
        Assert.True(first != third);
    }

    [Fact]
    public void ImageRedactionRequestAddsUniqueRegions()
    {
        var request = new ImageRedactionRequest(new byte[] { 1, 2, 3 });
        var region = new BoundingBox(0, 0, 5, 5);

        request.AddRegion(region);
        request.AddRegion(region);

        Assert.Single(request.Regions);
    }

    [Fact]
    public void ImageRedactionRequestAcceptsDifferentRegions()
    {
        var request = new ImageRedactionRequest(new byte[] { 1, 2, 3 });

        request.AddRegion(new BoundingBox(0, 0, 5, 5));
        request.AddRegion(new BoundingBox(2, 2, 3, 3));

        Assert.Equal(2, request.Regions.Count);
    }

    [Fact]
    public void ImageRedactionResultStoresRegions()
    {
        var region = new BoundingBox(0, 0, 5, 5);
        var result = new ImageRedactionResult(new byte[] { 1, 2, 3 }, new[] { region });

        Assert.Single(result.RedactedRegions);
        Assert.Equal(region, result.RedactedRegions[0]);
    }

    [Fact]
    public void ImageRedactionResultThrowsWhenImageEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ImageRedactionResult(ReadOnlyMemory<byte>.Empty, Array.Empty<BoundingBox>()));
    }
}
