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
    public class LineGeometry
    { 
        /// <summary>
        /// Type
        /// </summary>
        [Required]
        [DataMember(Name="type")]
        public string Type { get; set; }

        /// <summary>
        /// Geometry
        /// </summary>
        [Required]
        [DataMember(Name="geometry")]
        public LineGeometryGeometry Geometry { get; set; }

        /// <summary>
        /// Properties
        /// </summary>
        [Required]
        [DataMember(Name="properties")]
        public PointGeometryProperties Properties { get; set; }
    }
}