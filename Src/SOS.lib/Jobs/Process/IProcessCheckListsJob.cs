using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    /// <summary>
    /// Process check lists job
    /// </summary>
    public interface IProcessCheckListsJob
    {
        /// <summary>
        ///  Run full process job
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Process check lists for passed providers")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunAsync(
            IEnumerable<string> dataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);
    }
}