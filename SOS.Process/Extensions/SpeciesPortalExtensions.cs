using System.Collections.Generic;
using System.Linq;
using SOS.Process.Enums;
using SOS.Process.Models.Processed;
using SOS.Process.Models.Verbatim.SpeciesPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SpeciesPortalExtensions
    {
        /// <summary>
        /// Cast sighting verbatim to Darwin Core
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static DarwinCore ToDarwinCore(this APSightingVerbatim verbatim, IDictionary<string, DarwinCoreTaxon> taxa)
        {
            var taxonId = verbatim.TaxonId.ToString();
            return new DarwinCore()
            {
                DatasetID = $"{ (int)SightingProviders.SpeciesPortal }-{ verbatim.Id }",
                Taxon = taxa.ContainsKey(taxonId) ? taxa[taxonId] : null
            };
        }

        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore> ToDarwinCore(this IEnumerable<APSightingVerbatim> verbatims, IDictionary<string, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa));
        }
    }
}
