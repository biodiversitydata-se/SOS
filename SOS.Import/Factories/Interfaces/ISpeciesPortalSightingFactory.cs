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
        /// Aggregate all areas
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestAreasAsync();
    }
}
