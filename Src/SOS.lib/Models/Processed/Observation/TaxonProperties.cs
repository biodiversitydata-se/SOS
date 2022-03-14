namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon properties record relation.
    /// </summary>    
    public class TaxonProperties<T>
    {
        public T TaxonId { get; set; }
        public int? SortOrder { get; set; }
        public int? TaxonCategoryId { get; set; }
        public string TaxonCategorySwedishName { get; set; }
        public string TaxonCategoryEnglishName { get; set; }
        public string TaxonCategoryDarwinCoreName { get; set; }
    }
}
