using System.Collections.Generic;
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
        void AddAreaDataToProcessed(IEnumerable<ProcessedSighting> processedSightings);

        /// <summary>
        /// Save cache so we can use it after restart
        /// </summary>
        void PersistCache();
    }
}
