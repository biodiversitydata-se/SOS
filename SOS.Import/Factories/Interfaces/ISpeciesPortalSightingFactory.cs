using System.Threading.Tasks;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISpeciesPortalSightingFactory
    {
        /// <summary>
        /// Aggregate sightings
        /// </summary>
        /// <returns></returns>
        Task<bool> AggregateAsync();
    }
}
