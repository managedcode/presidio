using System.Text.Json.Serialization;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Describes a regular-expression based pattern used by <see cref="PatternRecognizer"/>.
/// Mirrors the Python analyzer contract.
/// </summary>
public sealed class Pattern
{
    [JsonConstructor]
    public Pattern(string name, string regex, double score)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(regex);

        Name = name;
        Regex = regex;
        Score = score;
    }

    public string Name { get; }

    public string Regex { get; }

    public double Score { get; }
}
