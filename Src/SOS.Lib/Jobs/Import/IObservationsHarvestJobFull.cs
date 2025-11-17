using Hangfire;
using SOS.Lib.HangfireAttributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import;

public interface IObservationsHarvestJobFull
{
    /// <summary>
    ///     Full harvest of all aktive data sets, start processing when done
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [JobExpirationTimeout(Minutes = 60 * 24 * 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
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
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    [JobDisplayName("Harvest observations from passed provides")]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("high")]
    Task<bool> RunHarvestObservationsAsync(
        List<string> harvestDataProviderIdOrIdentifiers,
        IJobCancellationToken cancellationToken);

    /// <summary>
    ///    Full harvest of iNaturalist observations.
    /// </summary>        
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    [JobDisplayName("Full iNaturalist harvest observations")]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("high")]
    Task<bool> RunFulliNaturalistHarvestObservationsAsync(
        IJobCancellationToken cancellationToken);
}