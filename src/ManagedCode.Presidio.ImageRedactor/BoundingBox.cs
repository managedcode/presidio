namespace ManagedCode.Presidio.ImageRedactor;

/// <summary>
/// Describes a rectangular region in image coordinates.
/// </summary>
public readonly struct BoundingBox : IEquatable<BoundingBox>
{
    public BoundingBox(int x, int y, int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }

    public int Right => X + Width;

    public int Bottom => Y + Height;

    public int Area => Width * Height;

    public bool Intersects(BoundingBox other)
    {
        return !(other.LeftOf(this) || other.RightOf(this) || other.Above(this) || other.Below(this));
    }

    private bool LeftOf(BoundingBox other) => Right <= other.X;

    private bool RightOf(BoundingBox other) => X >= other.Right;

    private bool Above(BoundingBox other) => Bottom <= other.Y;

    private bool Below(BoundingBox other) => Y >= other.Bottom;

    public bool Equals(BoundingBox other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is BoundingBox other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);

    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);
}
