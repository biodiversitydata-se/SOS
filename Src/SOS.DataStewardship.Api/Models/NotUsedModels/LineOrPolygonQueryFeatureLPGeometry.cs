using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models.NotUsedModels
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class LineOrPolygonQueryFeatureLPGeometry
    {
        /// <summary>
        /// Type enum
        /// </summary>
        public enum TypeEnum
        {
            /// <summary>
            /// Polygon
            /// </summary>
            [EnumMember(Value = "Polygon")]
            Polygon = 0,
            /// <summary>
            /// LineString
            /// </summary>
            [EnumMember(Value = "LineString")]
            LineString = 1
        }

        /// <summary>
        /// Type
        /// </summary>
        [DataMember(Name = "type")]
        public TypeEnum? Type { get; set; }
    }
}
