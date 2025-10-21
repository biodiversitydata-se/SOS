namespace SOS.Shared.Api.Dtos;

public class TaxonAreaAggDto
{     
    public int ObservationCount { get; set; }
    public Dictionary<string, int>? ObservationCountByAreaFeatureId { get; set; }
}