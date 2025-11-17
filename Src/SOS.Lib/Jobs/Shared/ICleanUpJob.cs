using Hangfire;
using SOS.Lib.HangfireAttributes;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Shared;

public interface ICleanUpJob
{
    /// <summary>
    /// Clean up old jobs
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [JobDisplayName("Clean up jobs")]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [JobExpirationTimeout(Minutes = 0)] // Prevent jobs to be stored to long in success 
    [Queue("low")]
    Task RunAsync(IJobCancellationToken cancellationToken);
}
