using System.Text.Json;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Represents the outcome of running an NLP engine over input text.
/// Mirrors the Python <c>NlpArtifacts</c> semantics used throughout Presidio.
/// </summary>
public sealed class NlpArtifacts
{
    private IReadOnlyList<string>? _lemmas;

    public NlpArtifacts(
        string language,
        IReadOnlyList<Token>? tokens = null,
        IReadOnlyList<string>? keywords = null,
        IReadOnlyList<double>? scores = null,
        IReadOnlyList<NlpEntity>? entities = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        Language = language;
        Tokens = tokens ?? Array.Empty<Token>();
        Keywords = keywords ?? Array.Empty<string>();
        Scores = scores ?? Array.Empty<double>();
        Entities = entities ?? Array.Empty<NlpEntity>();
    }

    public string Language { get; }

    public IReadOnlyList<Token> Tokens { get; }

    public IReadOnlyList<string> Keywords { get; }

    public IReadOnlyList<double> Scores { get; }

    public IReadOnlyList<NlpEntity> Entities { get; }

    public IReadOnlyList<string> Lemmas => _lemmas ??= Tokens.Select(token => token.Lemma).ToArray();

    public bool HasTokens => Tokens.Count > 0;

    public string ToJson()
    {
        var payload = new
        {
            language = Language,
            tokens = Tokens.Select(t => new { t.Text, t.Start, t.End, t.Lemma, t.PartOfSpeech }),
            lemmas = Lemmas,
            keywords = Keywords,
            scores = Scores,
            entities = Entities.Select(e => new
            {
                e.Label,
                e.Start,
                e.End,
                e.Score,
                e.RawLabel,
                e.Source,
                distribution = e.Distribution?.Vector.ToArray(),
            }),
        };

        return JsonSerializer.Serialize(payload);
    }
}
