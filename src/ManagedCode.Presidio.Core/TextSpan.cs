namespace ManagedCode.Presidio.Core;

/// <summary>
/// Represents a zero-based half-open interval within a larger text buffer.
/// </summary>
public readonly struct TextSpan : IEquatable<TextSpan>, IComparable<TextSpan>
{
    public TextSpan(int start, int end)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "Start index must be non-negative.");
        }

        if (end < start)
        {
            throw new ArgumentOutOfRangeException(nameof(end), end, "End index must be greater than or equal to start index.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the inclusive start index of the span.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Gets the exclusive end index of the span.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Gets the length of the span measured in characters.
    /// </summary>
    public int Length => End - Start;

    public bool Contains(int index) => index >= Start && index < End;

    public bool Contains(TextSpan other) => Start <= other.Start && End >= other.End;

    public bool Intersects(TextSpan other) => IntersectionLength(other) > 0;

    public int IntersectionLength(TextSpan other)
    {
        if (End <= other.Start || other.End <= Start)
        {
            return 0;
        }

        return Math.Min(End, other.End) - Math.Max(Start, other.Start);
    }

    public TextSpan Union(TextSpan other)
    {
        var start = Math.Min(Start, other.Start);
        var end = Math.Max(End, other.End);
        return new TextSpan(start, end);
    }

    public int CompareTo(TextSpan other)
    {
        var comparison = Start.CompareTo(other.Start);
        return comparison != 0 ? comparison : End.CompareTo(other.End);
    }

    public bool Equals(TextSpan other) => Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => obj is TextSpan other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public override string ToString() => $"[{Start}, {End})";

    public static bool operator ==(TextSpan left, TextSpan right) => left.Equals(right);

    public static bool operator !=(TextSpan left, TextSpan right) => !left.Equals(right);

    public static bool operator <(TextSpan left, TextSpan right) => left.CompareTo(right) < 0;

    public static bool operator >(TextSpan left, TextSpan right) => left.CompareTo(right) > 0;

    public static bool operator <=(TextSpan left, TextSpan right) => left.CompareTo(right) <= 0;

    public static bool operator >=(TextSpan left, TextSpan right) => left.CompareTo(right) >= 0;
}
