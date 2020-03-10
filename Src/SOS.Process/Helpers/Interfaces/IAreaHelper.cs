using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IAreaHelper
    {
        /// <summary>
        /// Add area data to processed sightings model
        /// </summary>
        /// <param name="processedSightings"></param>
        /// <returns></returns>
        void AddAreaDataToProcessedSightings(IEnumerable<ProcessedSighting> processedSightings);

        /// <summary>
        /// Add area data to processed sighting model
        /// </summary>
        /// <param name="processedSighting"></param>
        void AddAreaDataToProcessedSighting(ProcessedSighting processedSighting);

        /// <summary>
        /// Save cache so we can use it after restart
        /// </summary>
        void PersistCache();
    }
}
