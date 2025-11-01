namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Factory contract for materialising NLP engines from configuration.
/// </summary>
public interface INlpEngineFactory
{
    string Name { get; }

    bool IsAvailable { get; }

    INlpEngine Create(NlpEngineConfiguration configuration);
}

