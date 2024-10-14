using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonAttributeService
{
    public class TaxonAttributeResponse
    {
        /// <summary>
        /// Taxon attributes
        /// </summary>
        public IEnumerable<TaxonAttribute> TaxonAttributes { get; set; }

        /// <summary>
        /// Factors
        /// </summary>
        public IEnumerable<Factor> Factors { get; set; }
    }
}