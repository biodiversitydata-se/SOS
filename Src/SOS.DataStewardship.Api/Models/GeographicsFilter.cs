using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

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

        [DataMember(Name = "responseCoordinateSystem")]
        public CoordinateSystem ResponseCoordinateSystem { get; set; } // todo - this is not really a filter
    }
}