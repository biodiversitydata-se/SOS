using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Dataset info")]
    public class DatasetInfo
    {        
        [SwaggerSchema("Identifier")]
        public string Identifier { get; set; }
        
        [SwaggerSchema("Title")]
        public string Title { get; set; }
    }
}
