using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    public static class ProcessedTaxonExtensions
    {
        /// <summary>
        ///     Cast ProcessedTaxon objects to ProcessedBasicTaxon objects.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IBasicTaxon> ToProcessedBasicTaxa(this IEnumerable<Taxon> sourceTaxa)
        {
            return sourceTaxa?.Select(m => m.ToProcessedBasicTaxon());
        }

        /// <summary>
        ///     Cast ProcessedTaxon object to ProcessedBasicTaxon object.
        /// </summary>
        /// <param name="sourceTaxon"></param>
        /// <returns></returns>
        public static IBasicTaxon ToProcessedBasicTaxon(this Taxon sourceTaxon)
        {
            return new BasicTaxon
            {
                Attributes = sourceTaxon.Attributes,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.SecondaryParentDyntaxaTaxonIds,
                Id = sourceTaxon.Id,
                ScientificName = sourceTaxon.ScientificName
            };
        }
    }
}