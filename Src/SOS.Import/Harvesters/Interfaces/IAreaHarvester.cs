using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    /// Area harvester
    /// </summary>
    public interface IAreaHarvester
    {
        /// <summary>
        /// Aggregate all areas
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAreasAsync();

        /// <summary>
        /// Get all areas.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Area>> GetAreasAsync();

        /// <summary>
        /// Get all areas, but skip getting geometry field.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AreaBase>> GetAreasBaseAsync();
    }
}
