using SOS.DataStewardship.Api.Contracts.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Models
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
        public double? WeatherMeasure { get; set; }

        /// <summary>
        /// Unit for a reported measurement (given in the attribute "vädermått").
        /// </summary>
        [Required]
        public Unit? Unit { get; set; }
    }
}