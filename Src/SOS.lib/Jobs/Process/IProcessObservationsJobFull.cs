using Hangfire;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessObservationsJobFull
    {
        /// <summary>
        /// Run full process job
        /// </summary>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Process verbatim observations for all active providers")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}