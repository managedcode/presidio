using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Base class for entity recognizers, aligned with the Python EntityRecognizer contract.
/// </summary>
public abstract class EntityRecognizer
{
    public const double MinScore = 0;
    public const double MaxScore = 1.0;

    public IReadOnlyCollection<string> SupportedEntities { get; }

    public string Name { get; }

    public string Id { get; }

    public string SupportedLanguage { get; }

    public string Version { get; }

    public bool IsLoaded { get; private set; }

    public IReadOnlyCollection<string> Context { get; }

    protected EntityRecognizer(
        IEnumerable<string> supportedEntities,
        string? name = null,
        string supportedLanguage = "en",
        string version = "0.0.1",
        IEnumerable<string>? context = null)
    {
        SupportedEntities = supportedEntities?.ToArray() ?? throw new ArgumentNullException(nameof(supportedEntities));
        Name = name ?? GetType().Name;
        Id = $"{Name}_{Guid.NewGuid():N}";
        SupportedLanguage = supportedLanguage;
        Version = version;
        Context = context?.ToArray() ?? Array.Empty<string>();
    }

    public void Load() => EnsureLoaded();

    public IReadOnlyCollection<RecognizerResult> Analyze(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts)
    {
        EnsureLoaded();
        return AnalyzeCore(text, entities, artifacts);
    }

    public virtual IReadOnlyCollection<RecognizerResult> EnhanceUsingContext(
        string text,
        IReadOnlyCollection<RecognizerResult> ownResults,
        IReadOnlyCollection<RecognizerResult> otherResults,
        NlpArtifacts artifacts,
        IReadOnlyCollection<string>? context = null)
    {
        EnsureLoaded();
        return ownResults;
    }

    protected void EnsureLoaded()
    {
        if (IsLoaded)
        {
            return;
        }

        OnLoad();
        IsLoaded = true;
    }

    protected virtual void OnLoad()
    {
    }

    protected abstract IReadOnlyCollection<RecognizerResult> AnalyzeCore(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts);

    public static IReadOnlyCollection<RecognizerResult> RemoveDuplicates(IEnumerable<RecognizerResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return RecognizerResult.RemoveDuplicates(results);
    }

    public static string SanitizeValue(string text, IEnumerable<(string Search, string Replacement)> replacementPairs)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(replacementPairs);

        var sanitized = text;
        foreach (var (search, replacement) in replacementPairs)
        {
            if (!string.IsNullOrEmpty(search))
            {
                sanitized = sanitized.Replace(search, replacement ?? string.Empty, StringComparison.Ordinal);
            }
        }

        return sanitized;
    }

    public IReadOnlyDictionary<string, object?> ToDictionary() =>
        new Dictionary<string, object?>
        {
            { nameof(SupportedEntities), SupportedEntities },
            { nameof(SupportedLanguage), SupportedLanguage },
            { nameof(Name), Name },
            { nameof(Version), Version },
        };
}
