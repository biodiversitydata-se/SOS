using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

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
        Task<HarvestInfo> HarvestAreasAsync();
    }
}
