namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Contract for NLP engines capable of producing the artifacts consumed by the analyzer pipeline.
/// </summary>
public interface INlpEngine : IDisposable
{
    bool IsLoaded { get; }

    string PrimaryLanguage { get; }

    void Load();

    NlpArtifacts ProcessText(string text, string language);

    IReadOnlyCollection<string> GetSupportedLanguages();

    IReadOnlyCollection<string> GetSupportedEntities();

    bool IsStopWord(string token, string language);

    bool IsPunctuation(string token, string language);
}
