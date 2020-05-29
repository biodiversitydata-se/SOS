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
        public static IEnumerable<ProcessedBasicTaxon> ToProcessedBasicTaxa(this IEnumerable<ProcessedTaxon> sourceTaxa)
        {
            return sourceTaxa?.Select(m => m.ToProcessedBasicTaxon());
        }

        /// <summary>
        ///     Cast ProcessedTaxon object to ProcessedBasicTaxon object.
        /// </summary>
        /// <param name="sourceTaxon"></param>
        /// <returns></returns>
        public static ProcessedBasicTaxon ToProcessedBasicTaxon(this ProcessedTaxon sourceTaxon)
        {
            return new ProcessedBasicTaxon
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