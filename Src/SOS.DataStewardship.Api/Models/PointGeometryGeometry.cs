using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PointGeometryGeometry
    { 
        /// <summary>
        /// Type enum
        /// </summary>
        public enum TypeEnum
        {
            /// <summary>
            /// Point
            /// </summary>
            [EnumMember(Value = "Point")]
            Point = 0
        }

        /// <summary>
        /// Type
        /// </summary>
        [Required]
        [DataMember(Name="type")]
        public TypeEnum? Type { get; set; }

        /// <summary>
        /// Coordinates
        /// </summary>
        [Required]
        [DataMember(Name="coordinates")]
        public List<decimal?> Coordinates { get; set; }
    }
}
