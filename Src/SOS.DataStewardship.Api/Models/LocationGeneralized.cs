using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{    
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class LocationGeneralized
    {
        /// <summary>
        /// A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from \&quot;Stationsregistret\&quot; where relevant.
        /// </summary>
        [Required]
        [DataMember(Name = "locationID")]
        public string LocationID { get; set; }

        /// <summary>
        /// The name of a survey site or a subsite.
        /// </summary>
        [DataMember(Name = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.
        /// </summary>
        [DataMember(Name = "locationType")]
        public Location.LocationTypeEnum? LocationType { get; set; }

        /// <summary>
        /// Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.
        /// </summary>
        [Required]
        [DataMember(Name = "emplacement")]
        public IGeoShape Emplacement { get; set; }

        /// <summary>
        /// The province (swe: landskap) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [DataMember(Name = "stateProvince")]
        public string StateProvince { get; set; }

        /// <summary>
        /// County
        /// </summary>
        [DataMember(Name = "county")]
        public County County { get; set; }

        /// <summary>
        /// The province (swe: provins) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [DataMember(Name = "province")]
        public string Province { get; set; }

        /// <summary>
        /// Municipality
        /// </summary>
        [DataMember(Name = "municipality")]
        public Municipality Municipality { get; set; }

        /// <summary>
        /// The parish within which the site is situated. Should be derived from the given position.
        /// </summary>
        [DataMember(Name = "parish")]
        public string Parish { get; set; }

        /// <summary>
        /// Comment (freetext) from the survey event about the site.
        /// </summary>
        [DataMember(Name = "locationRemarks")]
        public string LocationRemarks { get; set; }
    }
}
