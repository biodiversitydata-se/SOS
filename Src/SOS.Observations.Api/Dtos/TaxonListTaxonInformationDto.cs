namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Taxon information.
    /// </summary>
    public class TaxonListTaxonInformationDto
    {
        /// <summary>
        /// Dyntaxa taxon id.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Scientific name.
        /// </summary>
        public string ScientificName { get; set; }
        
        /// <summary>
        /// Swedish name.
        /// </summary>
        public string SwedishName { get; set; }
    }
}