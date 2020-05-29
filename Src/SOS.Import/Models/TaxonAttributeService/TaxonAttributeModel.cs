using System.Collections.Generic;

namespace SOS.Import.Models.TaxonAttributeService
{
    /// <summary>
    ///     Taxon attribute model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaxonAttributeModel
    {
        /// <summary>
        ///     Attributes property
        /// </summary>
        public IEnumerable<FactorModel> Factors { get; set; }

        /// <summary>
        ///     Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     Category id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        ///     Host taxon id
        /// </summary>
        public int? HostTaxonId { get; set; }

        /// <summary>
        ///     Period
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        ///     Period id
        /// </summary>
        public int? PeriodId { get; set; }

        /// <summary>
        ///     Taxon id
        /// </summary>
        public int TaxonId { get; set; }
    }
}