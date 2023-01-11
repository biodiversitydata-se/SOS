using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.HangfireAttributes;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJob
    {
        /// <summary>
        ///     Full harvest of multiple sources, start processing when done
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
        ///  Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobExpirationTimeout(Minutes = 0)]
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [JobDisplayName("Incremental Harvest Observations, active instance")]
        [Queue("high")]
        Task<bool> RunIncrementalActiveAsync(DateTime? fromDate, IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [JobDisplayName("Incremental Harvest Observations, inactive instance")]
        [Queue("high")]
        Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="processDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(45)]
        [JobDisplayName("Harvest and process observations from passed provides")]
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
        [JobDisplayName("Harvest observations from passed provides")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken);

        [JobExpirationTimeout(Minutes = 0)]
        [JobDisplayName("Harvest specific observations from Artportalen")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunHarvestArtportalenObservationsAsync(
            IEnumerable<int> sightingIds,
            IJobCancellationToken cancellationToken);
    }
}