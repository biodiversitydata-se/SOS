using SOS.Lib.Dtos.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.Lib.Dtos.DataStewardship;

/// <summary>
/// Weather variable reported as a measurement and a unit.
/// </summary>
[DataContract]
public class DsWeatherMeasuringDto
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
    public DsUnit? Unit { get; set; }
}
