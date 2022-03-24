using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IAreaHelper
    {
        /// <summary>
        ///     Add area data to processed location model
        /// </summary>
        /// <param name="processedLocation"></param>
        void AddAreaDataToProcessedLocation(Location processedLocation);

        /// <summary>
        ///     Add area data to processed observation models
        /// </summary>
        /// <param name="processedLocations"></param>
        /// <returns></returns>
        void AddAreaDataToProcessedLocations(IEnumerable<Location> processedLocations);

        /// <summary>
        /// Add area data to site
        /// </summary>
        /// <param name="site"></param>
        void AddAreaDataToSite(Site site);

        /// <summary>
        /// Clear area cache
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Make sure cache is initialized
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();

        /// <summary>
        /// Return true if object is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Get areas of specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<IEnumerable<Models.Shared.Area>> GetAreasAsync(AreaType type);

        /// <summary>
        /// Get area geometry
        /// </summary>
        /// <param name="type"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<NetTopologySuite.Geometries.Geometry> GetGeometryAsync(AreaType type, string featureId);

        /// <summary>
        /// Get all features where position is inside feature
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        IEnumerable<IFeature> GetPointFeatures(Point point);
    }
}