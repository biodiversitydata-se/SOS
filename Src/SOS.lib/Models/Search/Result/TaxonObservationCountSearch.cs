using System.Collections.Generic;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Result
{
    /// <summary>
    /// Search query for calculating observation count for multiple taxa.
    /// </summary>
    public class TaxonObservationCountSearch
    {
        public IEnumerable<int> TaxonIds { get; set; }
        public bool IncludeUnderlyingTaxa { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public AreaType? AreaType { get; set; }
        public string FeatureId { get; set; }
        public int? DataProviderId { get; set; }
        //public IEnumerable<int> DataProviderIds { get; set; }
    }
}