using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    /// Geo related factory
    /// </summary>
    public interface IAreaHarvester
    {
        /// <summary>
        /// Aggregate all areas
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAreasAsync();
    }
}
