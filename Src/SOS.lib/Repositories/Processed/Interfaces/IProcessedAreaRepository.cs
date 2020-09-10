using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processed areas.
    /// </summary>
    public interface IProcessedAreaRepository : IMongoDbProcessedRepositoryBase<Area, int>
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

        Task<List<Area>> GetAsync(AreaType[] areaTypes);
    }
}