using System.Text.RegularExpressions;
using CoreRecognizerResult = ManagedCode.Presidio.Core.RecognizerResult;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Entry point for Presidio anonymization pipeline.
/// </summary>
public sealed class AnonymizerEngine : EngineBase
{
    private static readonly Regex WhitespaceRegex = new("^( )+$", RegexOptions.Compiled);

    public EngineResult Anonymize(
        string text,
        IReadOnlyCollection<CoreRecognizerResult>? analyzerResults,
        IDictionary<string, OperatorConfig>? operators = null,
        ConflictResolutionStrategy conflictResolution = ConflictResolutionStrategy.MergeSimilarOrContained)
    {
        ArgumentNullException.ThrowIfNull(text);
        var safeResults = analyzerResults ?? Array.Empty<CoreRecognizerResult>();
        ValidateIndices(text, safeResults);
        var mutableResults = CopyRecognizerResults(safeResults);
        if (mutableResults.Count == 0)
        {
            var defaultOperators = EnsureDefaultOperator(operators);
            return Operate(text, Array.Empty<PiiEntity>(), defaultOperators, OperatorType.Anonymize);
        }

        mutableResults.Sort();
        mutableResults = RemoveConflictsAndGetTextManipulationData(mutableResults, conflictResolution);
        mutableResults = MergeEntitiesWithWhitespaceBetween(text, mutableResults);
        var operatorMap = EnsureDefaultOperator(operators);

        return Operate(
            text,
            mutableResults.Cast<PiiEntity>().ToArray(),
            operatorMap,
            OperatorType.Anonymize);
    }

    public IReadOnlyCollection<string> GetAnonymizers() => OperatorsFactory.GetAnonymizers().Keys.ToArray();

    public void AddAnonymizer<T>() where T : Operator, new() => OperatorsFactory.AddAnonymizeOperator(typeof(T));

    public void AddAnonymizer(Type operatorType) => OperatorsFactory.AddAnonymizeOperator(operatorType);

    public void RemoveAnonymizer(Type operatorType) => OperatorsFactory.RemoveAnonymizeOperator(operatorType);

    private static void ValidateIndices(string text, IEnumerable<CoreRecognizerResult> analyzerResults)
    {
        foreach (var result in analyzerResults)
        {
            if (result.Start < 0 || result.Start > text.Length || result.End < 0 || result.End > text.Length)
            {
                throw new InvalidParamException(
                    $"Invalid analyzer result, start: {result.Start} and end: {result.End}, while text length is only {text.Length}.");
            }
        }
    }

    private static List<RecognizerResult> CopyRecognizerResults(IReadOnlyCollection<CoreRecognizerResult> analyzerResults)
    {
        var results = new List<RecognizerResult>(analyzerResults.Count);
        foreach (var result in analyzerResults)
        {
            results.Add(RecognizerResult.FromCore(result));
        }

        return results;
    }

    private static List<RecognizerResult> RemoveConflictsAndGetTextManipulationData(
        List<RecognizerResult> analyzerResults,
        ConflictResolutionStrategy conflictResolution)
    {
        var tmpAnalyzerResults = new List<RecognizerResult>();
        var otherElements = new List<RecognizerResult>(analyzerResults);

        foreach (var result in analyzerResults)
        {
            otherElements.Remove(result);

            var isMergeSameEntityType = false;
            foreach (var other in otherElements)
            {
                if (!string.Equals(result.EntityType, other.EntityType, StringComparison.Ordinal))
                {
                    continue;
                }

                if (result.Intersects(other) == 0)
                {
                    continue;
                }

                other.UpdateSpan(Math.Min(result.Start, other.Start), Math.Max(result.End, other.End));
                other.UpdateScore(Math.Max(result.Score, other.Score));
                isMergeSameEntityType = true;
                break;
            }

            if (!isMergeSameEntityType)
            {
                otherElements.Add(result);
                tmpAnalyzerResults.Add(result);
            }
        }

        var uniqueElements = new List<RecognizerResult>();
        otherElements = new List<RecognizerResult>(tmpAnalyzerResults);
        foreach (var result in tmpAnalyzerResults)
        {
            otherElements.Remove(result);
            if (!IsResultConflicted(otherElements, result))
            {
                otherElements.Add(result);
                uniqueElements.Add(result);
            }
        }

        if (conflictResolution == ConflictResolutionStrategy.RemoveIntersections)
        {
            uniqueElements.Sort((a, b) => a.Start.CompareTo(b.Start));
            var index = 0;
            while (index < uniqueElements.Count - 1)
            {
                var current = uniqueElements[index];
                var next = uniqueElements[index + 1];

                if (current.End <= next.Start)
                {
                    index += 1;
                    continue;
                }

                if (current.Score >= next.Score)
                {
                    if (current.End >= next.End)
                    {
                        uniqueElements.RemoveAt(index + 1);
                        continue;
                    }

                    next.UpdateSpan(current.End, next.End);
                }
                else
                {
                    if (next.Start <= current.Start)
                    {
                        uniqueElements.RemoveAt(index);
                        continue;
                    }

                    current.UpdateSpan(current.Start, next.Start);
                }

                uniqueElements.Sort((a, b) => a.Start.CompareTo(b.Start));
                index = 0;
            }

            uniqueElements = uniqueElements
                .Where(element => element.Start <= element.End)
                .ToList();
        }

        return uniqueElements;
    }

    private static List<RecognizerResult> MergeEntitiesWithWhitespaceBetween(
        string text,
        List<RecognizerResult> analyzerResults)
    {
        var mergedResults = new List<RecognizerResult>();
        RecognizerResult? previous = null;
        foreach (var result in analyzerResults)
        {
            if (previous is not null &&
                string.Equals(previous.EntityType, result.EntityType, StringComparison.Ordinal) &&
                IsWhitespace(text, previous.End, result.Start))
            {
                mergedResults.Remove(previous);
                result.UpdateSpan(previous.Start, result.End);
            }

            mergedResults.Add(result);
            previous = result;
        }

        return mergedResults;
    }

    private static bool IsWhitespace(string text, int start, int end)
    {
        if (start < 0 || end > text.Length || end <= start)
        {
            return false;
        }

        var span = text[start..end];
        return WhitespaceRegex.IsMatch(span);
    }

    private static bool IsResultConflicted(IEnumerable<RecognizerResult> others, RecognizerResult candidate)
    {
        foreach (var other in others)
        {
            if (candidate.HasConflict(other))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, OperatorConfig> EnsureDefaultOperator(IDictionary<string, OperatorConfig>? operators)
    {
        var map = new Dictionary<string, OperatorConfig>(StringComparer.Ordinal);
        if (operators is not null)
        {
            foreach (var pair in operators)
            {
                map[pair.Key] = pair.Value;
            }
        }

        if (!map.ContainsKey("DEFAULT"))
        {
            map["DEFAULT"] = new OperatorConfig("replace");
        }

        return map;
    }
}
