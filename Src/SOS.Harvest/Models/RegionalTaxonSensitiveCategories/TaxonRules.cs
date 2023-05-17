namespace SOS.Harvest.Models.RegionalTaxonSensitiveCategories
{
    internal class TaxonRules
    {
        /// <summary>
        /// Id of taxon to protect
        /// </summary>
        public int TaxonId { get; set; }

        /// <summary>
        /// Protection rules
        /// </summary>
        public IEnumerable<Rule>? Rules { get; set; }
    }
}
