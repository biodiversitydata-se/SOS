using System.Threading.Tasks;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Geo related factory
    /// </summary>
    public interface IGeoFactory
    {
        /// <summary>
        /// Aggregate all areas
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestAreasAsync();
    }
}
