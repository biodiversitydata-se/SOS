namespace SOS.Api.Contracts.Aggregations.Responses;
/// <summary>Observation counts and sighting dates for one taxon.</summary>
public class TaxonAggregationItemDto
{

    /// <summary>The first recorded sighting date, preserving the server's date-time representation.</summary>
    public System.DateTime? FirstSighting { get; set; }

    /// <summary>The last recorded sighting date, preserving the server's date-time representation.</summary>
    public System.DateTime? LastSighting { get; set; }

    /// <summary>The Dyntaxa identifier of the taxon.</summary>
    public int TaxonId { get; set; }

    /// <summary>The number of matching observations for the taxon.</summary>
    public int ObservationCount { get; set; }

}


