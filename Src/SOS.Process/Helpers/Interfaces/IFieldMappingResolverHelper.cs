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
        /// <param name="processedSightings"></param>
        void ResolveFieldMappedValues(IEnumerable<ProcessedSighting> processedSightings);
        
        /// <summary>
        /// Resolve field mapped values.
        /// </summary>
        /// <param name="processedSightings"></param>
        /// <param name="cultureCode"></param>
        void ResolveFieldMappedValues(IEnumerable<ProcessedSighting> processedSightings, string cultureCode);
    }
}
