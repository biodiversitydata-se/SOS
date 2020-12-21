using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// Area cache interface
    /// </summary>
    public interface IAreaCache : ICache<string, Area>
    {
        /// <summary>
        /// Get list of areas
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<Area>> GetAreasAsync(IEnumerable<(AreaType areaType, string featureId)> areaKeys);

        /// <summary>
        ///     Get all the areas, paged
        /// </summary>
        /// <param name="areaTypes">Skip this many</param>
        /// <param name="searchString">Skip this many</param>
        /// <param name="skip">Skip this many</param>
        /// <param name="take">Take this many areas</param>
        /// <returns></returns>
        Task<PagedResult<Area>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString, int skip,
            int take);

        /// <summary>
        /// Try to get cached entity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<Area> GetAsync(AreaType type, string featureId);

        /// <summary>
        ///  Get the geometry for a area
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId);

        /// Get multiple geometries
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<IGeoShape>> GetGeometriesAsync(IEnumerable<(AreaType areaType, string featureId)> areaKeys);
    }
}
