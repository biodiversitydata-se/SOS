using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Weather variable reported as a measurement and a unit.
    /// </summary>
    [DataContract]
    public class WeatherMeasuringDto
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
