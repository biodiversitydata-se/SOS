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
    public class PointOrBoundingBoxQueryFeaturePBBGeometry
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
            PointEnum = 0,
            /// <summary>
            /// BoundingBox
            /// </summary>
            [EnumMember(Value = "BoundingBox")]
            BoundingBoxEnum = 1
        }

        /// <summary>
        /// Type
        /// </summary>
        [DataMember(Name = "type")]
        public TypeEnum? Type { get; set; }
    }
}
