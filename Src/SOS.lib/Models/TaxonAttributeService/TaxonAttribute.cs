using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonAttributeService
{
    /// <summary>
    ///     Taxon attribute model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaxonAttribute
    {
        /// <summary>
        ///     Attributes property
        /// </summary>
        public IEnumerable<TaxonAttributeValue> Values { get; set; }
        
        /// <summary>
        ///     Factor id
        /// </summary>
        public int? FactorId { get; set; }

        /// <summary>
        ///     Taxon id
        /// </summary>
        public int TaxonId { get; set; }
    }
}