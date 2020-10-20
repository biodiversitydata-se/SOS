using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processed areas.
    /// </summary>
    public interface IAreaRepository : IMongoDbProcessedRepositoryBase<Area, int>
    {
        /// <summary>
        ///     Create indexes
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        ///     Delete all geometries stored in Gridfs
        /// </summary>
        /// <returns></returns>
        Task DropGeometriesAsync();

        /// <summary>
        ///     Get the geometry for a area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task<IGeoShape> GetGeometryAsync(int areaId);

        /// <summary>
        ///     Get all the areas, paged
        /// </summary>
        /// <param name="areaTypes">Skip this many</param>
        /// <param name="searchString">Skip this many</param>
        /// <param name="skip">Skip this many</param>
        /// <param name="take">Take this many areas</param>
        /// <returns></returns>
        public Task<PagedResult<Area>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString, int skip,
            int take);

        Task<List<Area>> GetAsync(AreaType[] areaTypes);
    }
}