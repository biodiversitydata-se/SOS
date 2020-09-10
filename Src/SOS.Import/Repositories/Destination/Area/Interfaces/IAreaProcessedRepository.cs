using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SOS.Import.Repositories.Resource.Interfaces;

namespace SOS.Import.Repositories.Destination.Area.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IAreaProcessedRepository : IResourceRepositoryBase<SOS.Lib.Models.Shared.Area, int>
    {
        /// <summary>
        ///     Delete all geometries stored in Gridfs
        /// </summary>
        /// <returns></returns>
        Task DropGeometriesAsync();

        /// <summary>
        ///     Save geometries to Gridfs
        /// </summary>
        /// <param name="areaGeometries"></param>
        /// <returns></returns>
        Task<bool> StoreGeometriesAsync(IDictionary<int, Geometry> areaGeometries);
    }
}