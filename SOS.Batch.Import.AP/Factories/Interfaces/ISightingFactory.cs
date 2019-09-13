using System.Threading.Tasks;

namespace SOS.Batch.Import.AP.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Aggregate sightings
        /// </summary>
        /// <returns></returns>
        Task<bool> AggregateAsync();
    }
}
