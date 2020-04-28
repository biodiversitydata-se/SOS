using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Models.Area;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get information about a single area
        /// </summary>
        /// <returns></returns>
        Task<Area> GetAreaAsync(int areaId);

        /// <summary>
        /// Get areas matching provided filter
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="nameFilter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<ExternalArea>> GetAreasAsync(AreaType areaType, string nameFilter, int skip, int take);
    }
}
