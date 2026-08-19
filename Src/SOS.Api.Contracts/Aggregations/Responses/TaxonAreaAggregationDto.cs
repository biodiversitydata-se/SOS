namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>Observation counts for a taxon, optionally grouped by area feature.</summary>
public sealed class TaxonAreaAggregationDto
{
    /// <summary>The observation count for the taxon.</summary>
    public int ObservationCount { get; set; }

    /// <summary>The observation count including summed descendants.</summary>
    public int SumObservationCount { get; set; }

    /// <summary>Observation counts keyed by area feature identifier.</summary>
    public Dictionary<string, int>? ObservationCountByAreaFeatureId { get; set; }

    /// <summary>Summed observation counts keyed by area feature identifier.</summary>
    public Dictionary<string, int>? SumObservationCountByAreaFeatureId { get; set; }
}
