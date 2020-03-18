using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.ObservationHarvesters.Interfaces
{
    /// <summary>
    /// Sighting factory repository
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
