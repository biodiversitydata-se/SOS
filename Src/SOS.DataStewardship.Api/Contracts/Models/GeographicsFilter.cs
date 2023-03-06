using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Geometry filter.
    /// </summary>
    [DataContract]
    public class GeographicsFilter
    {
        [DataMember(Name = "county")]
        public County? County { get; set; }

        [DataMember(Name = "municipality")]
        public Municipality? Municipality { get; set; }

        [DataMember(Name = "parish")]
        public Parish? Parish { get; set; }

        [DataMember(Name = "province")]
        public Province? Province { get; set; }

        [DataMember(Name = "geometry")]
        public GeographicsFilterArea Geometry { get; set; }
    }
}