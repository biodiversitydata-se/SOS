using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    /// <summary>
    /// Process checklists job
    /// </summary>
    public interface IProcessChecklistsJob
    {
        /// <summary>
        ///  Run full process job
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Process checklists for passed providers")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunAsync(
            IEnumerable<string> dataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);
    }
}