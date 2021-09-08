using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class ExtendedAuthorizationFilter
    {
        /// <summary>
        /// If UserId > 0 and ObservedByMe or ReportedByMe are true
        /// </summary>
        public bool AllowViewOwn => ViewOwn && UserId > 0;

        /// <summary>
        /// User has extended authorization
        /// </summary>
        public bool AllowProtected => AllowViewOwn || (ExtendedAreas?.Any() ?? false);

        /// <summary>
        /// If this is true, ObservedByMe and ReportedByMe is not forced.
        /// All observations the user has authorization to view will be returned (incl. the ones the user has reported or observed)
        /// </summary>
        public bool NotOnlyOwn { get; set; }

        /// <summary>
        /// Areas where user has extended authorization
        /// </summary>
        public IEnumerable<ExtendedAuthorizationAreaFilter> ExtendedAreas{ get; set; }

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

        /// <summary>
        /// True if ObservedByMe or ReportedByMe is set
        /// </summary>
        public bool ViewOwn => ObservedByMe || ReportedByMe;
    }
}