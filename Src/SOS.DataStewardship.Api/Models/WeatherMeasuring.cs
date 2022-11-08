using SOS.DataStewardship.Api.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Weather variable reported as a measurement and a unit.
    /// </summary>
    [DataContract]
    public class WeatherMeasuring
    { 
        /// <summary>
        /// Value for measured weather variable.
        /// </summary>
        [Required]
        [DataMember(Name="weatherMeasure")]
        public decimal? WeatherMeasure { get; set; }

        /// <summary>
        /// Unit for a reported measurement (given in the attribute "vädermått").
        /// </summary>
        [Required]
        [DataMember(Name="unit")]
        public Unit? Unit { get; set; }
    }
}
