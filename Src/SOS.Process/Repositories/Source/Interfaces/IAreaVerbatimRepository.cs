using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IAreaVerbatimRepository : IVerbatimBaseRepository<Area, int>
    {
        /// <summary>
        /// Get all areas of requested type where passed coordinates are inside the area
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        Task<IEnumerable<Area>> GetAreasByCoordinatesAsync(double longitude, double latitude);

        /// <summary>
        /// Get all areas, but skip getting the geometry field.
        /// </summary>
        /// <returns></returns>
        Task<List<AreaBase>> GetAllAreaBaseAsync();
    }
}
