using System.Threading.Tasks;
using SOS.Import.Models;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISpeciesPortalSightingFactory
    {
        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestSightingsAsync();

        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <param name="options">Options used in aggregation.</param>
        /// <returns></returns>
        Task<bool> HarvestSightingsAsync(SpeciesPortalAggregationOptions options);
    }
}
