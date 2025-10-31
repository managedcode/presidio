using ManagedCode.Presidio.Core;
using Microsoft.Extensions.AI;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Represents a named entity produced by the NLP engine.
/// </summary>
public sealed record NlpEntity(
    string Label,
    int Start,
    int End,
    double Score,
    string? RawLabel = null,
    string? Source = null,
    Embedding<float>? Distribution = null)
{
    public TextSpan ToSpan() => new(Start, End);
}
