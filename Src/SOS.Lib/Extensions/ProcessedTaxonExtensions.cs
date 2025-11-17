using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Extensions;

public static class ProcessedTaxonExtensions
{
    extension(IEnumerable<Taxon> sourceTaxa)
    {
        /// <summary>
        ///     Cast ProcessedTaxon objects to ProcessedBasicTaxon objects.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBasicTaxon> ToProcessedBasicTaxa()
        {
            return sourceTaxa?.Select(m => m.ToProcessedBasicTaxon());
        }
    }

    extension(Taxon sourceTaxon)
    {
        /// <summary>
        ///     Cast ProcessedTaxon object to ProcessedBasicTaxon object.
        /// </summary>
        /// <returns></returns>
        public IBasicTaxon ToProcessedBasicTaxon()
        {
            return new BasicTaxon
            {
                Attributes = sourceTaxon.Attributes,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.SecondaryParentDyntaxaTaxonIds,
                Id = sourceTaxon.Id,
                ScientificName = sourceTaxon.ScientificName,
                ScientificNameAuthorship = sourceTaxon.ScientificNameAuthorship,
                VernacularName = sourceTaxon.VernacularName
            };
        }
    }
}