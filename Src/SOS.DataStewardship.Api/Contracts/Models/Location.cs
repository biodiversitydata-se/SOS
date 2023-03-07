using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Information about a place that has been surveyed.")]
    public class Location
    {        
        [Required]
        [SwaggerSchema("A unique id-number for a place, e.g. a survey site or a subsite. Should be the id-number from Stationsregistret where relevant.")]
        public string LocationID { get; set; }
        
        [SwaggerSchema("The name of a survey site or a subsite.")]
        public string Locality { get; set; }
        
        [SwaggerSchema("The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.")]
        public enum LocationTypeEnum            
        {
            /// <summary>
            ///  Block for block
            /// </summary>
            [EnumMember(Value = "block")]
            Block = 0,
            /// <summary>
            ///  Linje for linje
            /// </summary>
            [EnumMember(Value = "linje")]
            Linje = 1,
            /// <summary>
            ///  Linjetransekt for linjetransekt
            /// </summary>
            [EnumMember(Value = "linjetransekt")]
            Linjetransekt = 2,
            /// <summary>
            ///  Polygon for polygon
            /// </summary>
            [EnumMember(Value = "polygon")]
            Polygon = 3,
            /// <summary>
            ///  Provyta for provyta
            /// </summary>
            [EnumMember(Value = "provyta")]
            Provyta = 4,
            /// <summary>
            ///  Punkt for punkt
            /// </summary>
            [EnumMember(Value = "punkt")]
            Punkt = 5,
            /// <summary>
            ///  Punktlokal for punktlokal
            /// </summary>
            [EnumMember(Value = "punktlokal")]
            Punktlokal = 6,
            /// <summary>
            ///  Ruta for ruta
            /// </summary>
            [EnumMember(Value = "ruta")]
            Ruta = 7,
            /// <summary>
            ///  Rutt for rutt
            /// </summary>
            [EnumMember(Value = "rutt")]
            Rutt = 8,
            /// <summary>
            ///  Rkningssektor for räkningssektor
            /// </summary>
            [EnumMember(Value = "räkningssektor")]
            Räkningssektor = 9,
            /// <summary>
            ///  Rkningszon for räkningszon
            /// </summary>
            [EnumMember(Value = "räkningszon")]
            Räkningszon = 10,
            /// <summary>
            ///  Segment for segment
            /// </summary>
            [EnumMember(Value = "segment")]
            Segment = 11,
            /// <summary>
            ///  Slinga for slinga
            /// </summary>
            [EnumMember(Value = "slinga")]
            Slinga = 12,
            /// <summary>
            ///  Transportstrcka for transportsträcka
            /// </summary>
            [EnumMember(Value = "transportsträcka")]
            Transportsträcka = 13,
            /// <summary>
            ///   for ö
            /// </summary>
            [EnumMember(Value = "ö")]
            Ö = 14
        }
        
        [SwaggerSchema("The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.")]
        public LocationTypeEnum? LocationType { get; set; }
        
        [Required]
        [SwaggerSchema("Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.")]        
        public IGeoShape Emplacement { get; set; }
        
        [Required]
        [SwaggerSchema("County")]        
        public County County { get; set; }
        
        [SwaggerSchema("The province (swe: provins) within which the site is situated. Should be derived from the given position.")]
        public Province Province { get; set; }

        [Required]
        [SwaggerSchema("Municipality")]
        public Municipality Municipality { get; set; }
        
        [SwaggerSchema("The parish within which the site is situated. Should be derived from the given position.")]
        public Parish Parish { get; set; }
        
        [SwaggerSchema("Comment (freetext) from the survey event about the site.")]
        public string LocationRemarks { get; set; }
    }
}