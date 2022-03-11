namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon sort order record relation.
    /// </summary>    
    public class TaxonSortOrder<T>
    { 
        public T TaxonId { get; set; }
        public int? SortOrder { get; set; }        
    }
}
