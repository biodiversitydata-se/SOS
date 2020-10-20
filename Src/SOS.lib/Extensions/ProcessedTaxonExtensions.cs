using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    public static class ProcessedTaxonExtensions
    {
        /// <summary>
        ///     Cast ProcessedTaxon objects to ProcessedBasicTaxon objects.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<BasicTaxon> ToProcessedBasicTaxa(this IEnumerable<Taxon> sourceTaxa)
        {
            return sourceTaxa?.Select(m => m.ToProcessedBasicTaxon());
        }

        /// <summary>
        ///     Cast ProcessedTaxon object to ProcessedBasicTaxon object.
        /// </summary>
        /// <param name="sourceTaxon"></param>
        /// <returns></returns>
        public static BasicTaxon ToProcessedBasicTaxon(this Taxon sourceTaxon)
        {
            return new BasicTaxon
            {
                DyntaxaTaxonId = sourceTaxon.DyntaxaTaxonId,
                ParentDyntaxaTaxonId = sourceTaxon.ParentDyntaxaTaxonId,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.SecondaryParentDyntaxaTaxonIds,
                Id = sourceTaxon.Id,
                ScientificName = sourceTaxon.ScientificName
            };
        }
    }
}