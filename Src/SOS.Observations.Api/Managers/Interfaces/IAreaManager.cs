using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Models.Area;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get Area by type and feature id
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<Area> GetAsync(AreaType areaType, string featureId);

        /// <summary>
        ///     Get zipped json bytes with an area json file
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetZipppedAreaAsync(AreaType areaType, string feature);

        /// <summary>
        ///     Get areas matching provided filter
        /// </summary>
        /// <param name="areaTypes"></param>
        /// <param name="searchString"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<ExternalSimpleArea>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString,
            int skip, int take);
    }
}