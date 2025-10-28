using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class TaxonAreaAggregation
{
    public int ObservationCount { get; set; }
    public int SumObservationCount { get; set; }
    public Dictionary<string, int>? ObservationCountByAreaFeatureId { get; set; }
    public Dictionary<string, int>? SumObservationCountByAreaFeatureId { get; set; }
}