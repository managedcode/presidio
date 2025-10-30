using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Base entity describing a span of text targeted for anonymization.
/// </summary>
public abstract class PiiEntity(TextSpan span, string entityType) : IEquatable<PiiEntity>, IComparable<PiiEntity>
{
    public TextSpan Span { get; } = span;

    public string EntityType { get; } = string.IsNullOrWhiteSpace(entityType)
            ? throw new ArgumentException("Entity type must be provided.", nameof(entityType))
            : entityType;

    public int Start => Span.Start;

    public int End => Span.End;

    public int CompareTo(PiiEntity? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Span.CompareTo(other.Span);
    }

    public bool Equals(PiiEntity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Span.Equals(other.Span) && EntityType.Equals(other.EntityType, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as PiiEntity);

    public override int GetHashCode() => HashCode.Combine(Span, EntityType);

    public override string ToString() => $"{EntityType} [{Span.Start}, {Span.End})";

    public static bool operator >(PiiEntity left, PiiEntity right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) > 0;
    }

    public static bool operator <(PiiEntity left, PiiEntity right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) < 0;
    }

    public static bool operator >=(PiiEntity left, PiiEntity right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <=(PiiEntity left, PiiEntity right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) <= 0;
    }

    public static bool operator ==(PiiEntity? left, PiiEntity? right) =>
        ReferenceEquals(left, right) || left?.Equals(right) == true;

    public static bool operator !=(PiiEntity? left, PiiEntity? right) => !(left == right);
}
