using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IAreaHelper
    {
        /// <summary>
        ///     Add area data to processed observation models
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <returns></returns>
        void AddAreaDataToProcessedObservations(IEnumerable<Observation> processedObservations);

        /// <summary>
        ///     Add area data to processed observation model
        /// </summary>
        /// <param name="processedObservation"></param>
        void AddAreaDataToProcessedObservation(Observation processedObservation);

        /// <summary>
        /// Clear area cache
        /// </summary>
        void ClearCache();

        /// <summary>
        ///     Save cache so we can use it after restart
        /// </summary>
        void PersistCache();
    }
}