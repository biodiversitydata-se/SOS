using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Occurrence search filter.")]
    public class OccurrenceFilter
    {
        [SwaggerSchema("Area filter.")]
        public GeographicsFilter Area { get; set; }

        [SwaggerSchema("DatasetIds filter.")]
        public List<string> DatasetIds { get; set; }

        [SwaggerExclude]
        public List<string> DatasetList { get; set; }

        [SwaggerSchema("Date filter.")]
        public DateFilter DateFilter { get; set; }

        [SwaggerExclude]
        public DateFilter Datum { get; set; }

        [SwaggerSchema("EventIds filter.")]
        public List<string> EventIds { get; set; }

        [SwaggerSchema("Taxon filter.")]
        public TaxonFilter Taxon { get; set; }
        
       
    }
}
