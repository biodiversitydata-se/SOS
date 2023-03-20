using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    [SwaggerSchema("Dataset filter")]
    public class DatasetFilter
    {
        [SwaggerSchema("Area filter")]
        public GeographicsFilter Area { get; set; }

        [SwaggerSchema("DatasetIds filter")]
        public List<string> DatasetIds { get; set; }

        [SwaggerExclude]
        public List<string> DatasetList { get; set; }

        [SwaggerSchema("Date filter")]
        public DateFilter DateFilter { get; set; }

        [SwaggerExclude] 
        public DateFilter Datum { get; set; }

        [SwaggerSchema("Taxon filter")]
        public TaxonFilter Taxon { get; set; }
        
        
    }
}
