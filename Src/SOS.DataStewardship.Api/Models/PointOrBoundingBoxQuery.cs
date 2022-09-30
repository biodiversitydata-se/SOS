using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PointOrBoundingBoxQuery
    { 
        /// <summary>
        /// FeaturePBB
        /// </summary>
        [DataMember(Name="featurePBB")]
        public PointOrBoundingBoxQueryFeaturePBB FeaturePBB { get; set; }
    }
}
