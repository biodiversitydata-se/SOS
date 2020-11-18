using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJob
    {
        /// <summary>
        ///     Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Harvest Observations [Mode={0}]")]
        Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="processDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task<bool> RunAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            List<string> processDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Harvest multiple sources without starting processing.
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);
    }
}