using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Information about a place that has been surveyed.
    /// </summary>
    [DataContract]
    public class Location
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

        /// <summary>
        /// The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc.
        /// </summary>
        [DataMember(Name = "locationType")]
        public LocationTypeEnum? LocationType { get; set; }

        /// <summary>
        /// Information about the shape and geographic position of the site. It is possible to provide the geographic position of each site in two ways. The geographic position of a line- or polygon-shaped site can thereby be provided both as a point, e.g. the centroid, a corner or the start point (described by methodology), and as a line or polygon with coordinates for the full extent of the site. The geographic position of a point-shaped site is provided as a point.
        /// </summary>
        [Required]
        [DataMember(Name = "emplacement")]
        public IGeoShape Emplacement { get; set; }
        
        // Decide if we should use IGeoShape or GeometryObject data type for Emplacement.
        [DataMember(Name = "emplacementTest")]
        public GeometryObject EmplacementTest { get; set; }        

        /// <summary>
        /// County
        /// </summary>
        [Required]
        [DataMember(Name = "county")]
        public County County { get; set; }

        /// <summary>
        /// The province (swe: provins) within which the site is situated. Should be derived from the given position.
        /// </summary>
        [DataMember(Name = "province")]
        public string Province { get; set; } // todo - create enum

        /// <summary>
        /// Municipality
        /// </summary>
        [Required]
        [DataMember(Name = "municipality")]
        public Municipality Municipality { get; set; }

        /// <summary>
        /// The parish within which the site is situated. Should be derived from the given position.
        /// </summary>
        [DataMember(Name = "parish")]
        public string Parish { get; set; } // todo - create enum

        /// <summary>
        /// Comment (freetext) from the survey event about the site.
        /// </summary>
        [DataMember(Name = "locationRemarks")]
        public string LocationRemarks { get; set; }
    }
}