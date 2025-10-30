using System.Collections;
using System.Globalization;

namespace ManagedCode.Presidio.Core;

/// <summary>
/// Represents a single entity detection emitted by a recognizer.
/// </summary>
public sealed class RecognizerResult(
    string entityType,
    TextSpan span,
    double score,
    AnalysisExplanation? analysisExplanation = null,
    IDictionary<string, object?>? metadata = null) : IEquatable<RecognizerResult>, IComparable<RecognizerResult>
{
    public string EntityType { get; } = entityType ?? throw new ArgumentNullException(nameof(entityType));

    public TextSpan Span { get; } = span;

    public double Score { get; private set; } = score;

    public AnalysisExplanation? AnalysisExplanation { get; private set; } = analysisExplanation;

    public IDictionary<string, object?> Metadata { get; } = metadata is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(metadata);

    public int Start => Span.Start;

    public int End => Span.End;

    public int Length => Span.Length;

    public void UpdateScore(double score)
    {
        Score = score;
        AnalysisExplanation?.SetImprovedScore(score);
    }

    public void AppendAnalysisExplanationText(string text)
    {
        AnalysisExplanation?.AppendTextualExplanationLine(text);
    }

    public int IntersectionLength(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Span.IntersectionLength(other.Span);
    }

    public bool Intersects(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Span.Intersects(other.Span);
    }

    public bool Contains(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Span.Contains(other.Span);
    }

    public bool IsContainedIn(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return other.Span.Contains(Span);
    }

    public bool HasEqualIndices(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Span.Equals(other.Span);
    }

    public bool EqualIndices(RecognizerResult other) => HasEqualIndices(other);

    public bool HasConflict(RecognizerResult other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (EqualIndices(other))
        {
            return Score <= other.Score;
        }

        return other.Contains(this);
    }

    public int CompareTo(RecognizerResult? other)
    {
        if (other is null)
        {
            return 1;
        }

        var comparison = Span.CompareTo(other.Span);
        return comparison != 0 ? comparison : string.Compare(EntityType, other.EntityType, StringComparison.Ordinal);
    }

    public bool Equals(RecognizerResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EntityType.Equals(other.EntityType, StringComparison.Ordinal)
               && Span.Equals(other.Span)
               && Score.Equals(other.Score);
    }

    public override bool Equals(object? obj) => Equals(obj as RecognizerResult);

    public override int GetHashCode() => HashCode.Combine(EntityType, Span, Score);

    public override string ToString() =>
        $"type: {EntityType}, start: {Start}, end: {End}, score: {Score.ToString("G", CultureInfo.InvariantCulture)}";

    public static bool operator ==(RecognizerResult? left, RecognizerResult? right) =>
        ReferenceEquals(left, right) || left?.Equals(right) == true;

    public static bool operator !=(RecognizerResult? left, RecognizerResult? right) =>
        !(left == right);

    public static bool operator <(RecognizerResult left, RecognizerResult right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(RecognizerResult left, RecognizerResult right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(RecognizerResult left, RecognizerResult right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(RecognizerResult left, RecognizerResult right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.CompareTo(right) >= 0;
    }

    public static RecognizerResult FromDictionary(IReadOnlyDictionary<string, object?> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        static T GetRequired<T>(IReadOnlyDictionary<string, object?> source, string key, Func<object?, T> converter)
        {
            if (!source.TryGetValue(key, out var value))
            {
                throw new ArgumentException($"Missing required key '{key}'.", nameof(source));
            }

            return converter(value);
        }

        static string ConvertToString(object? value) =>
            value switch
            {
                null => string.Empty,
                string s => s,
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty,
            };

        static int ConvertToInt(object? value) => value switch
        {
            null => throw new ArgumentException("Value cannot be null when converting to int."),
            int i => i,
            long l => checked((int)l),
            double d => Convert.ToInt32(d, CultureInfo.InvariantCulture),
            float f => Convert.ToInt32(f, CultureInfo.InvariantCulture),
            string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) => result,
            IConvertible convertible => convertible.ToInt32(CultureInfo.InvariantCulture),
            _ => throw new ArgumentException("Unsupported value type when converting to int."),
        };

        static double ConvertToDouble(object? value) => value switch
        {
            null => 0d,
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            decimal m => (double)m,
            string s when double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) => result,
            IConvertible convertible => convertible.ToDouble(CultureInfo.InvariantCulture),
            _ => throw new ArgumentException("Unsupported value type when converting to double."),
        };

        var entityType = GetRequired(data, "entity_type", ConvertToString);
        var start = GetRequired(data, "start", ConvertToInt);
        var end = GetRequired(data, "end", ConvertToInt);
        var score = GetRequired(data, "score", ConvertToDouble);

        Dictionary<string, object?>? metadata = null;
        if (data.TryGetValue("metadata", out var metadataValue) && metadataValue is not null)
        {
            if (metadataValue is IDictionary<string, object?> typed)
            {
                metadata = new Dictionary<string, object?>(typed);
            }
            else if (metadataValue is IDictionary legacyDictionary)
            {
                metadata = new Dictionary<string, object?>(legacyDictionary.Count);
                foreach (DictionaryEntry entry in legacyDictionary)
                {
                    if (entry.Key is string key)
                    {
                        metadata[key] = entry.Value;
                    }
                }
            }
        }

        return new RecognizerResult(entityType, new TextSpan(start, end), score, null, metadata);
    }

    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>(Metadata)
        {
            [nameof(EntityType)] = EntityType,
            [nameof(Start)] = Start,
            [nameof(End)] = End,
            [nameof(Score)] = Score,
        };
    }
}
