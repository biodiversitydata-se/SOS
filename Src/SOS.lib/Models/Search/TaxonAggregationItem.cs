namespace SOS.Lib.Models.Search
{
    public class TaxonAggregationItem
    {
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }

        public static TaxonAggregationItem Create(int taxonId, int count)
        {
            return new TaxonAggregationItem
            {
                TaxonId = taxonId,
                ObservationCount = count
            };
        }
    }
}