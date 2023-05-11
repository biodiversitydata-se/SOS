namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Taxon object
    /// </summary>
    public class TaxonEntity
    {        
        /// <summary>
        ///     Id of taxon
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Species group id
        /// </summary>
        public int? SpeciesGroupId { get; set; }      
    }
}