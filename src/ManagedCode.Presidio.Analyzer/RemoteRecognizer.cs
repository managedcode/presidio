using ManagedCode.Presidio.Core;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Base class for recognizers that delegate detection to an out-of-process or remote service.
/// Mirrors the Python <c>RemoteRecognizer</c> semantics.
/// </summary>
public abstract class RemoteRecognizer(
    IEnumerable<string> supportedEntities,
    string? name = null,
    string supportedLanguage = "en",
    string version = "0.0.1",
    IEnumerable<string>? context = null) : EntityRecognizer(supportedEntities, name, supportedLanguage, version, context)
{

    /// <summary>
    /// Implementations must call the remote service and translate its response into recognizer results.
    /// </summary>
    protected abstract IReadOnlyCollection<RecognizerResult> AnalyzeRemote(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts);

    public override IReadOnlyCollection<RecognizerResult> EnhanceUsingContext(
        string text,
        IReadOnlyCollection<RecognizerResult> ownResults,
        IReadOnlyCollection<RecognizerResult> otherResults,
        NlpArtifacts artifacts,
        IReadOnlyCollection<string>? context = null)
    {
        // Remote recognizers usually do not perform context enhancement locally,
        // but derived classes can override this if they post-process remote responses.
        return ownResults;
    }

    protected override IReadOnlyCollection<RecognizerResult> AnalyzeCore(
        string text,
        IReadOnlyCollection<string> entities,
        NlpArtifacts artifacts)
    {
        return AnalyzeRemote(text, entities, artifacts);
    }
}
