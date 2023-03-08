using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    [SwaggerSchema("Dataset filter")]
    public class DatasetFilter
    {                
        [SwaggerSchema("DatasetIds filter")]
        public List<string> DatasetIds { get; set; }
        
        [SwaggerSchema("Date filter")]
        public DateFilter DateFilter { get; set; }
        
        [SwaggerSchema("Taxon filter")]
        public TaxonFilter Taxon { get; set; }
        
        [SwaggerSchema("Area filter")]
        public GeographicsFilter Area { get; set; }
    }
}
