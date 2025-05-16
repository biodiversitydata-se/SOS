﻿using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IVocabularyValueResolver
    {
        /// <summary>
        /// Vocabulary configuration.
        /// </summary>
        VocabularyConfiguration Configuration { get; }

        /// <summary>
        ///     Resolve vocabulary mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="forceResolve">Ignore configuration setting, that can prevent resolve, and force resolve of values.</param>
        void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations, bool forceResolve = false);

        /// <summary>
        ///     Resolve vocabulary mapped values.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="cultureCode"></param>
        /// <param name="forceResolve">Ignore configuration setting, that can prevent resolve, and force resolve of values.</param>
        void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations, string cultureCode, bool forceResolve = false);

        /// <summary>
        ///     Resolve vocabulary mapped values.
        /// </summary>
        /// <param name="processedRecords"></param>
        /// <param name="cultureCode"></param>
        /// <param name="forceResolve">Ignore configuration setting, that can prevent resolve, and force resolve of values.</param>
        public void ResolveVocabularyMappedValues(IEnumerable<JsonNode> processedRecords,
            string cultureCode,
            bool forceResolve = false);
    }
}