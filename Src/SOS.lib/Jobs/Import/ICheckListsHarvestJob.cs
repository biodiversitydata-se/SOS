using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;


namespace SOS.Lib.Jobs.Import
{
    public interface ICheckListsHarvestJob
    {
        /// <summary>
        ///     Full harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Harvest Check Lists")]
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}