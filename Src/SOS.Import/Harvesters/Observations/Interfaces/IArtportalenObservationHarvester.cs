using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations.Interfaces
{
    /// <summary>
    ///     Artportalen observation harvester interface
    /// </summary>
    public interface IArtportalenObservationHarvester
    {
        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestSightingsAsync(JobRunModes mode, IJobCancellationToken cancellationToken);
    }
}