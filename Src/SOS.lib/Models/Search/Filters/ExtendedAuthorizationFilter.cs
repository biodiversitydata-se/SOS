using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class ExtendedAuthorizationFilter
    {
        /// <summary>
        /// Areas where user has extended authorization
        /// </summary>
        public List<ExtendedAuthorizationAreaFilter> ExtendedAreas { get; set; }

        /// <summary>
        /// Only get observations observed by me
        /// </summary>
        public bool ObservedByMe { get; set; }

        /// <summary>
        /// Only include ProtectedObservations
        /// </summary>
        public bool ProtectedObservations { get; set; }

        /// <summary>
        /// Only get observations reported by me
        /// </summary>
        public bool ReportedByMe { get; set; }

        /// <summary>
        /// Id of user making the request
        /// </summary>
        public int UserId { get; set; }
    }
}