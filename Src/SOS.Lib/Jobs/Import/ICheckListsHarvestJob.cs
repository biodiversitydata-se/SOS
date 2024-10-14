using Hangfire;
using System.ComponentModel;
using System.Threading.Tasks;


namespace SOS.Lib.Jobs.Import
{
    public interface IChecklistsHarvestJob
    {
        /// <summary>
        ///     Full harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Harvest Checklists")]
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}