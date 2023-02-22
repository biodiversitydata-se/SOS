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
    public class LineGeometryGeometry
    {
        /// <summary>
        /// Type enum
        /// </summary>
        public enum TypeEnum
        {
            /// <summary>
            /// LineString
            /// </summary>
            [EnumMember(Value = "LineString")]
            LineString = 0
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
        public List<List<double?>> Coordinates { get; set; }
    }
}
