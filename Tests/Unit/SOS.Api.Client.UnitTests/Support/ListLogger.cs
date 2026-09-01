using Microsoft.Extensions.Logging;

namespace SOS.Api.Client.UnitTests.Support;

internal sealed class ListLogger<T>(LogLevel minimumLevel = LogLevel.Trace) : ILogger<T>
{
    private readonly List<LogEntry> _entries = [];
    private int _isEnabledCallCount;

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_entries)
            {
                return _entries.ToArray();
            }
        }
    }

    public int IsEnabledCallCount => Volatile.Read(ref _isEnabledCallCount);

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        Interlocked.Increment(ref _isEnabledCallCount);
        return logLevel != LogLevel.None && logLevel >= minimumLevel;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var properties = state is IEnumerable<KeyValuePair<string, object?>> values
            ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
            : new Dictionary<string, object?>();

        lock (_entries)
        {
            _entries.Add(new LogEntry(
                logLevel,
                eventId,
                formatter(state, exception),
                properties));
        }
    }
}

internal sealed record LogEntry(
    LogLevel Level,
    EventId EventId,
    string Message,
    IReadOnlyDictionary<string, object?> Properties);
