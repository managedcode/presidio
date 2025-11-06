namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Handles textual replacements while preserving original indices semantics.
/// </summary>
internal sealed class TextReplaceBuilder
{
    private int _lastReplacementIndex;

    public TextReplaceBuilder(string originalText)
    {
        ArgumentNullException.ThrowIfNull(originalText);

        OriginalText = originalText;
        OutputText = originalText;
        _lastReplacementIndex = originalText.Length;
    }

    public string OriginalText { get; }

    public string OutputText { get; private set; }

    private int TextLength => OriginalText.Length;

    public string GetTextInPosition(int start, int end)
    {
        ValidatePositionInText(start, end);
        return OriginalText[start..end];
    }

    public int ReplaceTextGetInsertionIndex(string? replacementText, int start, int end)
    {
        ValidatePositionInText(start, end);

        var effectiveEnd = Math.Min(end, _lastReplacementIndex);
        _lastReplacementIndex = start;

        var beforeText = OutputText[..start];
        var afterText = OutputText[effectiveEnd..];
        OutputText = string.Concat(beforeText, replacementText, afterText);

        return afterText.Length + (replacementText?.Length ?? 0);
    }

    private void ValidatePositionInText(int start, int end)
    {
        if (start < 0 || start > TextLength)
        {
            throw new InvalidParamException(
                $"Invalid analyzer result, start: {start} and end: {end}, while text length is only {TextLength}.");
        }

        if (end < 0)
        {
            throw new InvalidParamException(
                $"Invalid analyzer result, start: {start} and end: {end}, while text length is only {TextLength}.");
        }

        var maxEnd = TextLength + 1;
        if (end > maxEnd)
        {
            throw new InvalidParamException(
                $"Invalid analyzer result, start: {start} and end: {end}, while text length is only {TextLength}.");
        }
    }
}
