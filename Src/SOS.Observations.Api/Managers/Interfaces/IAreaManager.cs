using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get areas by id
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<AreaBaseDto>> GetAreasAsync(IEnumerable<(AreaTypeDto, string)> areaKeys);

        /// <summary>
        ///     Get areas matching provided filter
        /// </summary>
        /// <param name="areaTypes"></param>
        /// <param name="searchString"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<AreaBaseDto>> GetAreasAsync(IEnumerable<AreaTypeDto> areaTypes, string searchString,
            int skip, int take);

        /// <summary>
        /// Get zipped json bytes with an area json file
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<byte[]> GetZipppedAreaAsync(AreaTypeDto areaType, string featureId);

        /// <summary>
        /// Get a area geometry
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId);

        /// <summary>
        /// Get multiple geometries
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<IGeoShape>> GetGeometriesAsync(IEnumerable<(AreaType areaType, string featureId)> areaKeys);
    }
}