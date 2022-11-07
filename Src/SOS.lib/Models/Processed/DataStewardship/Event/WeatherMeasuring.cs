using SOS.Lib.Models.Processed.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.Lib.Models.Processed.Event
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
        [DataMember(Name = "weatherMeasure")]
        public decimal? WeatherMeasure { get; set; }

        /// <summary>
        /// Unit for a reported measurement (given in the attribute "vädermått").
        /// </summary>
        [Required]
        [DataMember(Name = "unit")]
        public Unit? Unit { get; set; }
    }
}