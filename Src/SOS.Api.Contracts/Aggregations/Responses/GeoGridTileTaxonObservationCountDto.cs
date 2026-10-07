namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// The number of observations for a taxon in a map grid cell.
/// </summary>
public class GeoGridTileTaxonObservationCountDto
{
    /// <summary>The Dyntaxa identifier of the taxon.</summary>
    public int TaxonId { get; set; }
    /// <summary>The number of observations for the taxon in the cell.</summary>
    public int ObservationCount { get; set; }
}
