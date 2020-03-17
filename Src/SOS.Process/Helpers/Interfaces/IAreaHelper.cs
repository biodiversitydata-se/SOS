using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IAreaHelper
    {
        /// <summary>
        /// Add area data to processed observation models
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <returns></returns>
        void AddAreaDataToProcessedObservations(IEnumerable<ProcessedObservation> processedObservations);

        /// <summary>
        /// Add area data to processed observation model
        /// </summary>
        /// <param name="processedObservation"></param>
        void AddAreaDataToProcessedObservation(ProcessedObservation processedObservation);

        /// <summary>
        /// Save cache so we can use it after restart
        /// </summary>
        void PersistCache();
    }
}
