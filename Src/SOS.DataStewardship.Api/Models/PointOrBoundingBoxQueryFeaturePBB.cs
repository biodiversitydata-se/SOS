using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PointOrBoundingBoxQueryFeaturePBB
    { 
        /// <summary>
        /// Geometry
        /// </summary>
        [DataMember(Name="geometry")]
        public PointOrBoundingBoxQueryFeaturePBBGeometry Geometry { get; set; }
    }
}
