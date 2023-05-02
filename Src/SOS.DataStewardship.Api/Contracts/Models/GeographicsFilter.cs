using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.Lib.Swagger;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    /// <summary>
	/// Geographics filter
	/// </summary>
    public class GeographicsFilter
    {
        [SwaggerExclude]
        public GeometryFilter Area { get; set; }

        /// <summary>
		/// County filter
		/// </summary>
        public County? County { get; set; }

        /// <summary>
		/// Geometry filter
		/// </summary>
        public GeometryFilter Geometry { get; set; }

        /// <summary>
		/// Municipality filter
		/// </summary>
        public Municipality? Municipality { get; set; }

        /// <summary>
		/// Parish filter
		/// </summary>
        public Parish? Parish { get; set; }

        /// <summary>
		/// Province filter
		/// </summary>
        public Province? Province { get; set; }
    }
}