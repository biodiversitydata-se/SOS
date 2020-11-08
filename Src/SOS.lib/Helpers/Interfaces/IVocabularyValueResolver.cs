using System.Collections.Generic;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IVocabularyValueResolver
    {
        /// <summary>
        /// Field mapping configuration.
        /// </summary>
        VocabularyConfiguration Configuration { get; }

        /// <summary>
        ///     Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations);

        /// <summary>
        ///     Resolve field mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="cultureCode"></param>
        void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations, string cultureCode);
    }
}