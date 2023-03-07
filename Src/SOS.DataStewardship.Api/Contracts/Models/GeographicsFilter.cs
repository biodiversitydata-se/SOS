using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    [SwaggerSchema("Geographics filter")]
    public class GeographicsFilter
    {        
        [SwaggerSchema("County filter")]
        public County? County { get; set; }

        [SwaggerSchema("Municipality filter")]
        public Municipality? Municipality { get; set; }

        [SwaggerSchema("Parish filter")]
        public Parish? Parish { get; set; }

        [SwaggerSchema("Province filter")]
        public Province? Province { get; set; }

        [SwaggerSchema("Geometry filter")]
        public GeometryFilter Geometry { get; set; }
    }
}