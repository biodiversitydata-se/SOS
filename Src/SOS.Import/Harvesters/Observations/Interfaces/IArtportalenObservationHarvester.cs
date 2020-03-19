using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations.Interfaces
{
    /// <summary>
    /// Artportalen observation harvester interface
    /// </summary>
    public interface IArtportalenObservationHarvester
    {
        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestSightingsAsync(IJobCancellationToken cancellationToken);
    }
}
