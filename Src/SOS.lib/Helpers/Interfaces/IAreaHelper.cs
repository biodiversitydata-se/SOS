using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;

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
    }
}