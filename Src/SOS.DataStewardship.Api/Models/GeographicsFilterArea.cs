using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Geographic Area
    /// </summary>
    [DataContract]
    public class GeographicsFilterArea
    { 
        /// <summary>
        /// GeographicArea
        /// </summary>
        [DataMember(Name="geographicArea")]
        public IGeoShape GeographicArea { get; set; }

        // Decide if geographicArea should use IGeoShape or GeometryObject.
        [DataMember(Name = "geographicAreaTest")]
        public GeometryObject GeographicAreaTest { get; set; }

        /// <summary>
        /// The offset in meters from the geometries. This variable is required if geometries type is point
        /// </summary>
        [DataMember(Name="maxDistanceFromGeometries")]
        public double? MaxDistanceFromGeometries { get; set; }
    }
}
