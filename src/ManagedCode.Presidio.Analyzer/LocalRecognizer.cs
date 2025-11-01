namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Marker base class for recognizers that execute within the analyzer process.
/// Maintains parity with the Python <c>LocalRecognizer</c> contract.
/// </summary>
public abstract class LocalRecognizer(
    IEnumerable<string> supportedEntities,
    string? name = null,
    string supportedLanguage = "en",
    string version = "0.0.1",
    IEnumerable<string>? context = null) : EntityRecognizer(supportedEntities, name, supportedLanguage, version, context)
{
}

