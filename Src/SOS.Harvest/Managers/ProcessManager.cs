using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SOS.Harvest.Managers;

public class ProcessManager : IProcessManager
{
    private readonly SemaphoreSlim _threadHandler;
    private readonly ILogger<ProcessManager> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(60);
    private TimeSpan _maxWaitDuration = new TimeSpan();
    private int _waitingCount = 0;
    private readonly ConcurrentDictionary<string, DateTime> _contextTimestamps = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processConfiguration"></param>
    public ProcessManager(ProcessConfiguration processConfiguration, ILogger<ProcessManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var noOfThreads = processConfiguration?.NoOfThreads ?? throw new ArgumentNullException(nameof(processConfiguration));
        _threadHandler = new SemaphoreSlim(noOfThreads, noOfThreads);
    }

    /// <inheritdoc />
    public int Release(string context)
    {
        try
        {
            if (_contextTimestamps.TryRemove(context, out var waitTimestamp))
            {
                var elapsed = DateTime.UtcNow - waitTimestamp;
                _logger.LogInformation(
                    "ProcessManager released slot. Context={@context}, Available={@available}, ElapsedTime={@elapsed}, Waiting={@waiting}",
                    context, _threadHandler.CurrentCount, elapsed.ToString("hh\\:mm\\:ss"), Volatile.Read(ref _waitingCount));
            }
            else
            {
                _logger.LogWarning(
                    "ProcessManager released slot without matching WaitAsync. Context={@context}, Available={@available}, Waiting={@waiting}",
                    context, _threadHandler.CurrentCount, Volatile.Read(ref _waitingCount));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Release for Context={@context}", context);
        }

        var result = _threadHandler.Release();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> WaitAsync(string context)
    {            
        var currentWaiting = Interlocked.Increment(ref _waitingCount);
        _logger.LogInformation(
            "ProcessManager waiting for slot. Context={@context}, Available={@available}, Waiting={@waiting}",
            context, _threadHandler.CurrentCount, currentWaiting);

        var startTime = DateTime.UtcNow;

        try
        {
            var result = await _threadHandler.WaitAsync(_timeout);

            TimeSpan waitDuration = DateTime.UtcNow - startTime;
            if (waitDuration > _maxWaitDuration)
            {
                Interlocked.Exchange(ref Unsafe.As<TimeSpan, long>(ref _maxWaitDuration), waitDuration.Ticks);
                _logger.LogInformation(
                    "ProcessManager new max wait duration. Context={@context}, Available={@available}, WaitDuration={@waitDuration}, Waiting={@waiting}",
                    context, _threadHandler.CurrentCount, waitDuration.ToString("hh\\:mm\\:ss"), Volatile.Read(ref _waitingCount));
            }

            if (result)
            {                    
                _contextTimestamps[context] = DateTime.UtcNow;
                var leftWaiting = Interlocked.Decrement(ref _waitingCount);
                _logger.LogInformation(
                    "ProcessManager acquired slot. Context={@context}, Available={@available}, WaitDuration={@waitDuration}, Waiting={@waiting}",
                    context, _threadHandler.CurrentCount, waitDuration.ToString("hh\\:mm\\:ss"), leftWaiting);
            }
            else
            {                    
                var leftWaiting = Interlocked.Decrement(ref _waitingCount);
                _logger.LogError(
                    "ProcessManager timeout waiting for slot. Context={@context}, Available={@available}, WaitDuration={@waitDuration}, Waiting={@waiting}",
                    context, _threadHandler.CurrentCount, waitDuration.ToString("hh\\:mm\\:ss"), leftWaiting);
            }

            return result;
        }
        catch (Exception ex)
        {                
            Interlocked.Decrement(ref _waitingCount);
            TimeSpan waitDuration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Unexpected error in WaitAsync. Context={@context}, WaitDuration={@waitDuration}, Waiting={@waiting}",
                context, waitDuration.ToString("hh\\:mm\\:ss"), Volatile.Read(ref _waitingCount));
            throw;
        }
    }
}
