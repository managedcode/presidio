namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Factory for creating NLP engine instances aligned with Presidio defaults.
/// </summary>
public sealed class NlpEngineProvider(Func<INlpEngine>? engineFactory = null)
{
    private readonly Func<INlpEngine> _engineFactory = engineFactory ?? (() => new OnnxNlpEngine());

    public INlpEngine CreateEngine() => _engineFactory();
}
