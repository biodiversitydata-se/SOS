using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.Artportalen.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAreaVerbatimRepository : IVerbatimRepository<Area, int>
    {
        /// <summary>
        /// Delete all geometries stored in Gridfs
        /// </summary>
        /// <returns></returns>
        Task DropGeometriesAsync();

        /// <summary>
        /// Save geometries to Gridfs
        /// </summary>
        /// <param name="areaGeometries"></param>
        /// <returns></returns>
        Task<bool> StoreGeometriesAsync(IDictionary<int, Geometry> areaGeometries);
    }
}
