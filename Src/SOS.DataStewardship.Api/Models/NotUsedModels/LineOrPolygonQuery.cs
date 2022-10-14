using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models.NotUsedModels
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class LineOrPolygonQuery
    {
        /// <summary>
        /// FeatureLP
        /// </summary>
        [DataMember(Name = "featureLP")]
        public LineOrPolygonQueryFeatureLP FeatureLP { get; set; }
    }
}
