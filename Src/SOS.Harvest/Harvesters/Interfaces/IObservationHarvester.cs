using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces;

public interface IObservationHarvester
{
    /// <summary>
    ///  Harvest observations based on mode
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="mode"></param>
    /// <param name="fromDate"></param>
    /// <returns></returns>
    /// <param name="cancellationToken"></param>
    Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider, JobRunModes mode, DateTime? fromDate, IJobCancellationToken cancellationToken);

    /// <summary>
    ///  Harvest observations generic by provider
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken);

    /// <summary>
    /// Harvest observations slowly with delay in order to not overload the source.
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken);
}
