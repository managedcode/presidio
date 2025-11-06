namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Mutable representation of analyzer recognizer results tailored for the anonymizer engine.
/// </summary>
public sealed class RecognizerResult : PiiEntity, IComparable<RecognizerResult>
{
    public RecognizerResult(string entityType, int start, int end, double score)
        : base(start, end, entityType)
    {
        Validators.ValidateParameterExists(score, "analyzer result", "score");
        Score = score;
    }

    public double Score { get; private set; }

    public RecognizerResult Clone() => new(EntityType, Start, End, Score);

    public bool EqualIndices(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start == other.Start && End == other.End;
    }

    public bool Contains(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start <= other.Start && End >= other.End;
    }

    public int Intersects(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (End <= other.Start || other.End <= Start)
        {
            return 0;
        }

        return Math.Min(End, other.End) - Math.Max(Start, other.Start);
    }

    public bool HasConflict(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (EqualIndices(other))
        {
            return Score <= other.Score;
        }

        return other.Contains(this);
    }

    public void UpdateScore(double score)
    {
        Score = score;
    }

    public int CompareTo(RecognizerResult? other)
    {
        if (other is null)
        {
            return 1;
        }

        var comparison = Start.CompareTo(other.Start);
        return comparison != 0 ? comparison : End.CompareTo(other.End);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RecognizerResult other)
        {
            return false;
        }

        return base.Equals(other) && Score.Equals(other.Score);
    }

    public override int GetHashCode() => HashCode.Combine(Start, End, EntityType, Score);

    public static RecognizerResult FromCore(ManagedCode.Presidio.Core.RecognizerResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new RecognizerResult(result.EntityType, result.Start, result.End, result.Score);
    }

    public ManagedCode.Presidio.Core.RecognizerResult ToCore()
    {
        return new ManagedCode.Presidio.Core.RecognizerResult(
            EntityType,
            new ManagedCode.Presidio.Core.TextSpan(Start, End),
            Score);
    }
}
