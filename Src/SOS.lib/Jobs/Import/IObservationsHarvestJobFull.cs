using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.HangfireAttributes;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJobFull
    {
        /// <summary>
        ///     Full harvest of all aktive data sets, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobExpirationTimeout(Minutes = 60 * 24 * 3)]
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [JobDisplayName("Full Observations Harvest")]
        [Queue("high")]
        Task<bool> RunFullAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///    Full harvest multiple sources without starting processing.
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [JobDisplayName("Harvest observations from passed provides")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);
    }
}