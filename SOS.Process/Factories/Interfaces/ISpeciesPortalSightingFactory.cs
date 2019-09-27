using System.Threading.Tasks;

namespace SOS.Process.Factories.Interfaces
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
