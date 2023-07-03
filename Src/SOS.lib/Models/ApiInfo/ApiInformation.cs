using System;

namespace SOS.Lib.Models.ApiInfo
{
    /// <summary>
    /// API information.
    /// </summary>
    public class ApiInformation
    {
        /// <summary>
        /// Name of the API.
        /// </summary>        
        public string ApiName { get; set; }

        /// <summary>
        /// API version with MAJOR, MINOR and PATCH version.
        /// </summary>        
        public string ApiVersion { get; set; }

        /// <summary>
		/// The date when this API version was published.
		/// </summary>        
        public DateTimeOffset ApiReleased { get; set; }

        /// <summary>
		/// A link to the current API documentation.
		/// </summary>
        public Uri ApiDocumentation { get; set; }

        /// <summary>
		/// The state or status of the API according to lifecycle management. For example. alpha, beta, active, deprecated, retired or decommissioned.
		/// </summary>                
        public string ApiStatus { get; set; }
    }
}
