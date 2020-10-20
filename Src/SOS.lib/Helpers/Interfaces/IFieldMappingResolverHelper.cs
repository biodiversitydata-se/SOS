using System.Collections.Generic;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IFieldMappingResolverHelper
    {
        /// <summary>
        /// Field mapping configuration.
        /// </summary>
        FieldMappingConfiguration Configuration { get; }

        /// <summary>
        ///     Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        void ResolveFieldMappedValues(IEnumerable<Observation> processedObservations);

        /// <summary>
        ///     Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="cultureCode"></param>
        void ResolveFieldMappedValues(IEnumerable<Observation> processedObservations, string cultureCode);
    }
}