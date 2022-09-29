using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class LineOrPolygonQueryFeatureLP
    { 
        /// <summary>
        /// Geometry
        /// </summary>
        [DataMember(Name="geometry")]
        public LineOrPolygonQueryFeatureLPGeometry Geometry { get; set; }
    }
}
