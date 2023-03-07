using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Event dataset.")]
    public class EventDataset
    {        
        [SwaggerSchema("Identifier")]
        public string Identifier { get; set; }
        
        [SwaggerSchema("Title")]
        public string Title { get; set; }
    }
}
