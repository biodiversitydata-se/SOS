using System.ComponentModel.DataAnnotations;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
	/// Generalized location
	/// </summary>
    public class LocationGeneralized
    {        
        /// <summary>
		/// A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from \"Stationsregistret\" where relevant.
		/// </summary>        
        [Required]
        public string LocationID { get; set; }
        
        /// <summary>
		/// The name of a survey site or a subsite.
		/// </summary>
        public string Locality { get; set; }
        
        /// <summary>
		/// The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.
		/// </summary>
        public Enums.LocationType? LocationType { get; set; }

        /// <summary>
        /// Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.
        /// </summary>
        [Required]
        public IGeoShape Emplacement { get; set; }
        
        /// <summary>
		/// The province (swe: landskap) within which the site is situated. Should be derived from the given position.
		/// </summary>
        public string StateProvince { get; set; }
        
        /// <summary>
		/// County
		/// </summary>
        public County County { get; set; }
        
        /// <summary>
		/// The province (swe: provins) within which the site is situated. Should be derived from the given position.
		/// </summary>
        public string Province { get; set; }
        
        /// <summary>
		/// Municipality
		/// </summary>
        public Municipality Municipality { get; set; }
        
        /// <summary>
		/// The parish within which the site is situated. Should be derived from the given position.
		/// </summary>
        public string Parish { get; set; }
        
        /// <summary>
		/// Comment (freetext) from the survey event about the site.
		/// </summary>
        public string LocationRemarks { get; set; }
    }
}