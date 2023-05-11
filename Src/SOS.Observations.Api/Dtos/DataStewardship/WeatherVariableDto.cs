using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Weather variable
    /// </summary>
    public class WeatherVariableDto
    {
        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        public SnowCover? SnowCover { get; set; }

        /// <summary>
        /// States the amount of sunshine during the survey event.
        /// </summary>
        public WeatherMeasuringDto Sunshine { get; set; }

        /// <summary>
        /// States the air temperature during the survey event.
        /// </summary>
        public WeatherMeasuringDto AirTemperature { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a compass direction.
        /// </summary>
        public WindDirectionCompass? WindDirectionCompass { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a number of degrees.
        /// </summary>
        public WeatherMeasuringDto WindDirectionDegrees { get; set; }

        /// <summary>
        /// WindSpeed
        /// </summary>
        public WeatherMeasuringDto WindSpeed { get; set; }

        /// <summary>
        /// States the strength of the wind during the survey event.
        /// </summary>
        public WindStrength? WindStrength { get; set; }

        /// <summary>
        /// States the precipitation conditions during the survey event.
        /// </summary>
        public Precipitation? Precipitation { get; set; }

        /// <summary>
        /// States the visibility conditions during the survey event.
        /// </summary>
        public Visibility? Visibility { get; set; }

        /// <summary>
        /// States the cloud condtions during the survey event.
        /// </summary>
        public Cloudiness? Cloudiness { get; set; }
    }
}
