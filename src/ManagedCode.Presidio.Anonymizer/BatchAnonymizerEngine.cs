using System.Collections;
using System.Globalization;
using CoreRecognizerResult = ManagedCode.Presidio.Core.RecognizerResult;

namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Provides batch anonymization helpers mirroring the Python BatchAnonymizerEngine.
/// </summary>
public sealed class BatchAnonymizerEngine(AnonymizerEngine? anonymizerEngine = null)
{
    private readonly AnonymizerEngine _anonymizerEngine = anonymizerEngine ?? new AnonymizerEngine();

    public IReadOnlyList<object?> AnonymizeList(
        IReadOnlyList<object?> texts,
        IReadOnlyList<IReadOnlyList<RecognizerResult>> recognizerResultsList,
        IDictionary<string, OperatorConfig>? operators = null,
        ConflictResolutionStrategy conflictResolution = ConflictResolutionStrategy.MergeSimilarOrContained)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var results = NormalizeRecognizerResultsList(recognizerResultsList, texts.Count);
        var output = new List<object?>(texts.Count);

        for (var i = 0; i < texts.Count; i++)
        {
            var current = texts[i];
            if (current is string or bool or int or long or float or double or decimal)
            {
                var text = Convert.ToString(current, CultureInfo.InvariantCulture) ?? string.Empty;
                var engineResult = _anonymizerEngine.Anonymize(
                    text,
                    ConvertToCoreResults(results[i]),
                    operators,
                    conflictResolution);

                output.Add(engineResult.Text ?? string.Empty);
            }
            else
            {
                output.Add(current);
            }
        }

        return output;
    }

    public IDictionary<string, object?> AnonymizeDict(
        IEnumerable<DictRecognizerResult> analyzerResults,
        IDictionary<string, OperatorConfig>? operators = null,
        ConflictResolutionStrategy conflictResolution = ConflictResolutionStrategy.MergeSimilarOrContained)
    {
        ArgumentNullException.ThrowIfNull(analyzerResults);

        var result = new Dictionary<string, object?>();
        foreach (var entry in analyzerResults)
        {
            switch (entry.Value)
            {
                case IDictionary<string, object?>:
                {
                    var nestedResults = EnsureDictResults(entry.RecognizerResults);
                    result[entry.Key] = AnonymizeDict(nestedResults, operators, conflictResolution);
                    break;
                }

                case string stringValue:
                {
                    var recognizers = EnsureRecognizerResults(entry.RecognizerResults);
                    var engineResult = _anonymizerEngine.Anonymize(stringValue, ConvertToCoreResults(recognizers), operators, conflictResolution);
                    result[entry.Key] = engineResult.Text;
                    break;
                }

                case IEnumerable enumerable when entry.Value is not string:
                {
                    var recognizers = EnsureRecognizerResultsList(entry.RecognizerResults);
                    var values = enumerable.Cast<object?>().ToList();
                    var anonymized = AnonymizeList(values, recognizers, operators, conflictResolution);
                    result[entry.Key] = anonymized;
                    break;
                }

                default:
                    result[entry.Key] = entry.Value;
                    break;
            }
        }

        return result;
    }

    private static List<IReadOnlyList<RecognizerResult>> NormalizeRecognizerResultsList(
        IReadOnlyList<IReadOnlyList<RecognizerResult>> recognizerResultsList,
        int desiredLength)
    {
        var results = recognizerResultsList?.ToList() ?? new List<IReadOnlyList<RecognizerResult>>();
        if (results.Count == 0)
        {
            for (var i = 0; i < desiredLength; i++)
            {
                results.Add(Array.Empty<RecognizerResult>());
            }
        }
        else if (results.Count != desiredLength)
        {
            throw new ArgumentException("Recognizer results list must align with texts count.", nameof(recognizerResultsList));
        }

        return results;
    }

    private static IReadOnlyList<IReadOnlyList<RecognizerResult>> EnsureRecognizerResultsList(object? recognizerResults)
    {
        if (recognizerResults is null)
        {
            return Array.Empty<IReadOnlyList<RecognizerResult>>();
        }

        if (recognizerResults is IEnumerable<IReadOnlyList<RecognizerResult>> enumerable)
        {
            return enumerable.ToList();
        }

        throw new ArgumentException("Recognizer results must be a list of recognizer result collections for enumerable values.");
    }

    private static IReadOnlyList<RecognizerResult> EnsureRecognizerResults(object? recognizerResults)
    {
        if (recognizerResults is null)
        {
            return Array.Empty<RecognizerResult>();
        }

        if (recognizerResults is IEnumerable<RecognizerResult> enumerable)
        {
            return enumerable.ToList();
        }

        throw new ArgumentException("Recognizer results must be a list of recognizer results for string values.");
    }

    private static IEnumerable<DictRecognizerResult> EnsureDictResults(object? recognizerResults)
    {
        if (recognizerResults is null)
        {
            return Array.Empty<DictRecognizerResult>();
        }

        if (recognizerResults is IEnumerable<DictRecognizerResult> enumerable)
        {
            return enumerable;
        }

        throw new ArgumentException("Recognizer results must be an enumerable of DictRecognizerResult for nested dictionaries.");
    }

    private static IReadOnlyCollection<CoreRecognizerResult> ConvertToCoreResults(IReadOnlyList<RecognizerResult> results)
    {
        if (results.Count == 0)
        {
            return Array.Empty<CoreRecognizerResult>();
        }

        var converted = new List<CoreRecognizerResult>(results.Count);
        foreach (var result in results)
        {
            converted.Add(result.ToCore());
        }

        return converted;
    }
}
