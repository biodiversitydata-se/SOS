namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon county occurrence record relation.
    /// </summary>    
    public class TaxonCountyOccurrence<T>
    {
        public T TaxonId { get; set; }
        public int CountyId { get; set; }
        public string County { get; set; }
        public string Status { get; set; }
    }
}
