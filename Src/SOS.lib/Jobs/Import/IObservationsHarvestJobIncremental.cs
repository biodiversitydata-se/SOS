using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.HangfireAttributes;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJobIncremental
    {
        /// <summary>
        ///  Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DeleteRunningJobBeforeStart]
        [JobExpirationTimeout(Minutes = 0)] // Prevent jobs to be stored to long in success 
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [JobDisplayName("Incremental Harvest Observations, active instance")]
        [Queue("high")]
        Task<bool> RunIncrementalActiveAsync(DateTime? fromDate, IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Incremental harvest of multiple sources, start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DeleteRunningJobBeforeStart]
        [JobExpirationTimeout(Minutes = 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [JobDisplayName("Incremental Harvest Observations, inactive instance")]
        [Queue("high")]
        Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken);

        [JobExpirationTimeout(Minutes = 0)]
        [JobDisplayName("Harvest specific observations from Artportalen")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunHarvestArtportalenObservationsAsync(
            List<int> sightingIds,
            IJobCancellationToken cancellationToken);
    }
}