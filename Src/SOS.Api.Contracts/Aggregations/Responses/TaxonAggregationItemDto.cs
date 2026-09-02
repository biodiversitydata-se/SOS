namespace SOS.Api.Contracts.Aggregations.Responses;
public class TaxonAggregationItemDto
{

    public System.DateTimeOffset? FirstSighting { get; set; }

    public System.DateTimeOffset? LastSighting { get; set; }

    public int TaxonId { get; set; }

    public int ObservationCount { get; set; }

}


