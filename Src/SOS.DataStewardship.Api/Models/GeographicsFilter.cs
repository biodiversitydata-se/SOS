using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Models
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

        [DataMember(Name = "area")]
        public GeographicsFilterArea Area { get; set; }
    }
}