using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models.NotUsedModels
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PolygonGeometryGeometry
    {
        /// <summary>
        /// Type
        /// </summary>
        public enum TypeEnum
        {
            /// <summary>
            /// Polygon
            /// </summary>
            [EnumMember(Value = "Polygon")]
            PolygonEnum = 0
        }

        /// <summary>
        /// Type
        /// </summary>
        [Required]
        [DataMember(Name = "type")]
        public TypeEnum? Type { get; set; }

        /// <summary>
        /// Coordinates
        /// </summary>
        [Required]
        [DataMember(Name = "coordinates")]
        public List<List<List<double?>>> Coordinates { get; set; }
    }
}
