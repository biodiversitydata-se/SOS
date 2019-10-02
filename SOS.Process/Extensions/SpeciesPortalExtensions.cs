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
        /// <returns></returns>
        public static DarwinCore ToDarwinCore(this APSightingVerbatim verbatim)
        {
            return new DarwinCore()
            {
                DatasetID = $"{ (int)SightingProviders.SpeciesPortal }>{ verbatim.Id }",
                Taxon = verbatim.TaxonId != null ? new DarwinCoreTaxon
                {
                    TaxonID = verbatim.TaxonId.ToString()
                } : null
            };
        }

        /// <summary>
        ///  Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="activities"></param>
        /// <param name="genders"></param>
        /// <param name="stages"></param>
        /// <param name="units"></param>
        /// <param name="taxa"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore> ToDarwinCore(this IEnumerable<APSightingVerbatim> verbatims)
        {
            return verbatims.Select(v => v.ToDarwinCore());
        }
    }
}
