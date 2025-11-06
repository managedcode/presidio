namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Represents analyzer output for dictionary structured inputs.
/// Mirrors the Python <c>DictRecognizerResult</c> dataclass.
/// </summary>
public sealed class DictRecognizerResult(string key, object? value, object? recognizerResults)
{
    public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));

    public object? Value { get; } = value;

    public object? RecognizerResults { get; } = recognizerResults;
}
