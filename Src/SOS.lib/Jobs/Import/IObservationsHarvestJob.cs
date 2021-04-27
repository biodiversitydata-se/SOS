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
        ///     Full harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Full Harvest Observations")]
        Task<bool> RunFullAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Incremental Harvest Observations, active instance")]
        Task<bool> RunIncrementalActiveAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Incremental Harvest Observations, inactive instance")]
        Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="processDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
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
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);
    }
}