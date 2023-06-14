using Nest;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Information about a place that has been surveyed.
    /// </summary>
    [SwaggerSchema("Information about a place that has been surveyed.")]
    public class DsLocationDto
    {
        /// <summary>
        /// "A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from Stationsregistret where relevant.
        /// </summary>
        [Required]
        [SwaggerSchema("A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from Stationsregistret where relevant.")]
        public string LocationID { get; set; }

        /// <summary>
        /// The name of a survey site or a subsite.
        /// </summary>
        [SwaggerSchema("The name of a survey site or a subsite.")]
        public string Locality { get; set; }

        /// <summary>
        /// The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.
        /// </summary>
        [SwaggerSchema("The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.")]
        public DsLocationType LocationType { get; set; }

        /// <summary>
        /// Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. 
        /// The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), 
        /// and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.
        /// </summary>
        [Required]
        [SwaggerSchema("Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.")]
        public IGeoShape Emplacement { get; set; }

        /// <summary>
        /// The county (swe: län) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [Required]
        [SwaggerSchema("County")]
        public DsCounty County { get; set; }

        /// <summary>
        /// The province (swe: provins) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [SwaggerSchema("The province (swe: provins) within which the site is situated. Should be derived from the given position.")]
        public DsProvince Province { get; set; }

        /// <summary>
        /// The municipality (swe: kommun) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [Required]
        [SwaggerSchema("Municipality")]
        public Municipality Municipality { get; set; }

        /// <summary>
        /// The parish within which the site is situated. Should be derived from the given position.
        /// </summary>
        [SwaggerSchema("The parish within which the site is situated. Should be derived from the given position.")]
        public DsParish Parish { get; set; }

        /// <summary>
        /// Comment (freetext) from the survey event about the site.
        /// </summary>
        [SwaggerSchema("Comment (freetext) from the survey event about the site.")]
        public string LocationRemarks { get; set; }
    }
}