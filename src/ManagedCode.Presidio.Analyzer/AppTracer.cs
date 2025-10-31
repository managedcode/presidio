using System.Collections.Concurrent;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Provides request-scoped tracing similar to the Python <c>AppTracer</c>.
/// </summary>
public sealed class AppTracer
{
    private readonly ConcurrentQueue<(string? CorrelationId, string Message, DateTime Timestamp)> _entries = new();

    public IReadOnlyCollection<(string? CorrelationId, string Message, DateTime Timestamp)> Entries => _entries.ToArray();

    public void Trace(string? correlationId, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        _entries.Enqueue((correlationId, message, DateTime.UtcNow));
    }
}
