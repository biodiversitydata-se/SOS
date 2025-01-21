namespace SOS.Shared.Api.Dtos.Search
{
    public class AggregationItemOrganismQuantityDto
    {
        public string? AggregationKey { get; set; }
        public int DocCount { get; set; }
        public int OrganismQuantity { get; set; }
    }
}