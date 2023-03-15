using System.ComponentModel.DataAnnotations;
using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Generalized location")]
    public class LocationGeneralized
    {        
        [Required]
        [SwaggerSchema("A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from \"Stationsregistret\" where relevant.")]        
        public string LocationID { get; set; }
        
        [SwaggerSchema("The name of a survey site or a subsite.")]
        public string Locality { get; set; }
        
        [SwaggerSchema("The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.")]
        public Enums.LocationType? LocationType { get; set; }
        
        [Required]
        [SwaggerSchema("Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.")]
        public IGeoShape Emplacement { get; set; }
        
        [SwaggerSchema("The province (swe: landskap) within which the site is situated. Should be derived from the given position.")]
        public string StateProvince { get; set; }
        
        [SwaggerSchema("County")]
        public County County { get; set; }
        
        [SwaggerSchema("The province (swe: provins) within which the site is situated. Should be derived from the given position.")]
        public string Province { get; set; }
        
        [SwaggerSchema("Municipality")]
        public Municipality Municipality { get; set; }
        
        [SwaggerSchema("The parish within which the site is situated. Should be derived from the given position.")]
        public string Parish { get; set; }
        
        [SwaggerSchema("Comment (freetext) from the survey event about the site.")]
        public string LocationRemarks { get; set; }
    }
}