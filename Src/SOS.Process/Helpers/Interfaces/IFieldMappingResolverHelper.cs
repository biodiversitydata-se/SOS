using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IFieldMappingResolverHelper
    {
        /// <summary>
        /// Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations);
        
        /// <summary>
        /// Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="cultureCode"></param>
        void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations, string cultureCode);
    }
}
