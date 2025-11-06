using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Base entity describing a span of text targeted for anonymization operations.
/// Mirrors the mutable semantics of the Python implementation while exposing
/// <see cref="TextSpan"/> for consumption by the C# port.
/// </summary>
public abstract class PiiEntity : IEquatable<PiiEntity>, IComparable<PiiEntity>
{
    protected PiiEntity(int start, int end, string entityType)
    {
        Validators.ValidateParameterExists(start, "result", "start");
        Validators.ValidateType(start, "start", typeof(int));
        Validators.ValidateParameterExists(end, "result", "end");
        Validators.ValidateType(end, "end", typeof(int));
        Validators.ValidateParameterNotEmpty(entityType, "result", "entity_type");

        if (start < 0 || end < 0)
        {
            throw new InvalidParamException("Invalid input, result start and end must be positive");
        }

        if (start > end)
        {
            throw new InvalidParamException(
                $"Invalid input, start index '{start}' must be smaller than end index '{end}'");
        }

        EntityType = entityType;
        Span = new TextSpan(start, end);
    }

    protected PiiEntity(TextSpan span, string entityType)
        : this(span.Start, span.End, entityType)
    {
    }

    public string EntityType { get; }

    public int Start => Span.Start;

    public int End => Span.End;

    public TextSpan Span { get; private set; }

    public void UpdateSpan(int start, int end)
    {
        if (start < 0 || end < 0)
        {
            throw new InvalidParamException("Invalid input, result start and end must be positive");
        }

        if (start > end)
        {
            throw new InvalidParamException(
                $"Invalid input, start index '{start}' must be smaller than end index '{end}'");
        }

        Span = new TextSpan(start, end);
    }

    public int CompareTo(PiiEntity? other)
    {
        if (other is null)
        {
            return 1;
        }

        var comparison = Start.CompareTo(other.Start);
        return comparison != 0 ? comparison : End.CompareTo(other.End);
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

        return Start == other.Start && End == other.End &&
               EntityType.Equals(other.EntityType, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as PiiEntity);

    public override int GetHashCode() => HashCode.Combine(Start, End, EntityType);

    public override string ToString() => $"{EntityType} [{Start}, {End})";

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
