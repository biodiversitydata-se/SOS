using System.Threading.Tasks;

namespace SOS.Export.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Export all sightings
        /// </summary>
        /// <returns></returns>
        Task<bool> ExportAllAsync();

    }
}
