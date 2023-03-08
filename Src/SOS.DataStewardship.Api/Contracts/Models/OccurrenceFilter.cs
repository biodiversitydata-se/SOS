using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Occurrence search filter.")]
    public class OccurrenceFilter
    {        
        [SwaggerSchema("DatasetIds filter.")]
        public List<string> DatasetIds { get; set; }
        
        [SwaggerSchema("EventIds filter.")]
        public List<string> EventIds { get; set; }
        
        [SwaggerSchema("Date filter.")]
        public DateFilter DateFilter { get; set; }
        
        [SwaggerSchema("Taxon filter.")]
        public TaxonFilter Taxon { get; set; }
        
        [SwaggerSchema("Area filter.")]
        public GeographicsFilter Area { get; set; }
    }
}
