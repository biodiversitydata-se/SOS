using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Weather variable
    /// </summary>
    public class DsWeatherVariableDto
    {
        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        public SnowCover? SnowCover { get; set; }

        /// <summary>
        /// States the amount of sunshine during the survey event.
        /// </summary>
        public DsWeatherMeasuringDto Sunshine { get; set; }

        /// <summary>
        /// States the air temperature during the survey event.
        /// </summary>
        public DsWeatherMeasuringDto AirTemperature { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a compass direction.
        /// </summary>
        public DsWindDirectionCompass? WindDirectionCompass { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a number of degrees.
        /// </summary>
        public DsWeatherMeasuringDto WindDirectionDegrees { get; set; }

        /// <summary>
        /// WindSpeed
        /// </summary>
        public DsWeatherMeasuringDto WindSpeed { get; set; }

        /// <summary>
        /// States the strength of the wind during the survey event.
        /// </summary>
        public DsWindStrength? WindStrength { get; set; }

        /// <summary>
        /// States the precipitation conditions during the survey event.
        /// </summary>
        public DsPrecipitation? Precipitation { get; set; }

        /// <summary>
        /// States the visibility conditions during the survey event.
        /// </summary>
        public DsVisibility? Visibility { get; set; }

        /// <summary>
        /// States the cloud condtions during the survey event.
        /// </summary>
        public DsCloudiness? Cloudiness { get; set; }
    }
}
