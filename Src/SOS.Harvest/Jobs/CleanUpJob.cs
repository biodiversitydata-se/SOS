using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Shared;

namespace SOS.Harvest.Jobs;

/// <summary>
///     Artportalen harvest
/// </summary>
public class CleanUpJob : ICleanUpJob
{
    private readonly ILogger<CleanUpJob> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CleanUpJob(
        ILogger<CleanUpJob> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task RunAsync(IJobCancellationToken cancellationToken)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();

        var scheduledJobs = monitoringApi.ScheduledJobs(0, (int)monitoringApi.ScheduledCount());
        if (scheduledJobs?.Any() ?? false)
        {
            foreach (var keyValue in scheduledJobs)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var scheduledJob = keyValue.Value;
                if (string.IsNullOrEmpty(scheduledJob.InvocationData.Queue))
                {
                    var client = new BackgroundJobClient();
                    client.Requeue(keyValue.Key, "high");
                    continue;
                }
            }
        }

        foreach (var queue in monitoringApi.Queues())
        {
            var enqueuedJobs = monitoringApi.EnqueuedJobs(queue.Name, 0, (int)monitoringApi.EnqueuedCount(queue.Name));
            if (enqueuedJobs?.Any() ?? false)
            {
                foreach (var keyValue in enqueuedJobs)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var enqueuedJob = keyValue.Value;
                    if (string.IsNullOrEmpty(enqueuedJob.InvocationData.Queue))
                    {
                        var client = new BackgroundJobClient();
                        client.Requeue(keyValue.Key, "high");
                        continue;
                    }
                }
            }
        }

        var processingJobs = monitoringApi.ProcessingJobs(0, (int)monitoringApi.ProcessingCount());
        if (processingJobs?.Any() ?? false)
        {
            foreach (var keyValue in processingJobs)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var processingJob = keyValue.Value;
                if (processingJob.StartedAt.HasValue && processingJob.StartedAt.Value.ToUniversalTime() > DateTime.Now.ToUniversalTime().AddDays(1))
                {
                    var client = new BackgroundJobClient();
                    client.ChangeState(keyValue.Key, new FailedState(new Exception("Failed due to more than 24h run time")));
                    continue;
                }
            }
        }
    }
}