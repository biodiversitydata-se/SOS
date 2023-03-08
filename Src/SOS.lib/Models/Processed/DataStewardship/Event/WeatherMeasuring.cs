using SOS.Lib.Models.Processed.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;

namespace SOS.Lib.Models.Processed.DataStewardship.Event
{
    /// <summary>
    /// Weather variable reported as a measurement and a unit.
    /// </summary>
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