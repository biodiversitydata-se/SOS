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
        /// Unit for a reported measurement (given in the attribute \"vädermått\").
        /// </summary>
        public enum UnitEnum
        {
            /// <summary>
            /// %
            /// </summary>
            [EnumMember(Value = "%")]
            Percent = 0,
            /// <summary>
            /// cm²
            /// </summary>
            [EnumMember(Value = "cm²")]
            Cm2 = 1,
            /// <summary>
            /// cm³
            /// </summary>
            [EnumMember(Value = "cm³")]
            Cm3 = 2,
            /// <summary>
            /// dm²
            /// </summary>
            [EnumMember(Value = "dm²")]
            Dm2 = 3,
            /// <summary>
            /// kompassgrader
            /// </summary>
            [EnumMember(Value = "kompassgrader")]
            Kompassgrader = 4,
            /// <summary>
            /// m/s
            /// </summary>
            [EnumMember(Value = "m/s")]
            Ms = 5,
            /// <summary>
            /// m²
            /// </summary>
            [EnumMember(Value = "m²")]
            M2 = 6,
            /// <summary>
            /// styck
            /// </summary>
            [EnumMember(Value = "styck")]
            Styck = 7,
            /// <summary>
            /// °C
            /// </summary>
            [EnumMember(Value = "°C")]
            GraderCelsius = 8
        }

        /// <summary>
        /// Unit for a reported measurement (given in the attribute "vädermått").
        /// </summary>
        [Required]
        [DataMember(Name="unit")]
        public UnitEnum? Unit { get; set; }
    }
}
