using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessObservationsJobIncremental
    {
        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Process Observations [Mode={1}]")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> RunAsync(
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Process passed Artportalen verbatims
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        [JobDisplayName("Process passed Artportalen verbatim observations")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task<bool> ProcessArtportalenObservationsAsync(IEnumerable<ArtportalenObservationVerbatim> verbatims);
    }
}