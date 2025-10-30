namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Minimal token representation extracted by the NLP engine.
/// </summary>
public sealed class Token(string text, int start, int end, string lemma, string partOfSpeech)
{
    public string Text { get; } = text;

    public int Start { get; } = start;

    public int End { get; } = end;

    public string Lemma { get; } = lemma;

    public string PartOfSpeech { get; } = partOfSpeech;
}
