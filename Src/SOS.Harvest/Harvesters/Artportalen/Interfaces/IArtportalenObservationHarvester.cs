using Hangfire;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Harvesters.Artportalen.Interfaces
{
    /// <summary>
    ///     Artportalen observation harvester interface
    /// </summary>
    public interface IArtportalenObservationHarvester : IObservationHarvester
    {
        /// <summary>
        /// Harvest observations by id 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<ArtportalenObservationVerbatim>> HarvestObservationsAsync(IEnumerable<int> ids,
            IJobCancellationToken cancellationToken);
    }
}