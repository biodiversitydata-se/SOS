using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class TaxonAreaAgg
{
    public int ObservationCount { get; set; }
    public Dictionary<string, int>? ObservationCountByAreaFeatureId { get; set; }   
}