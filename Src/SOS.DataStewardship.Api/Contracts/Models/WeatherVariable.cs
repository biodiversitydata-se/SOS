using SOS.DataStewardship.Api.Contracts.Enums;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class WeatherVariable
    {
        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        public enum SnowCoverEnum
        {
            /// <summary>
            /// barmark
            /// </summary>
            [EnumMember(Value = "barmark")]
            Barmark = 0,
            /// <summary>
            /// snötäckt mark
            /// </summary>
            [EnumMember(Value = "snötäckt mark")]
            SnötäcktMark = 1,
            /// <summary>
            /// mycket tunt snötäcke eller fläckvis snötäcke
            /// </summary>
            [EnumMember(Value = "mycket tunt snötäcke eller fläckvis snötäcke")]
            MycketTuntSnötäckeEllerFläckvisSnötäcke = 2
        }

        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        [DataMember(Name = "snowCover")]
        public SnowCoverEnum? SnowCover { get; set; }

        /// <summary>
        /// States the amount of sunshine during the survey event.
        /// </summary>
        [DataMember(Name = "sunshine")]
        public WeatherMeasuring Sunshine { get; set; }

        /// <summary>
        /// States the air temperature during the survey event.
        /// </summary>
        [DataMember(Name = "airTemperature")]
        public WeatherMeasuring AirTemperature { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a compass direction.
        /// </summary>
        [DataMember(Name = "windDirectionCompass")]
        public WindDirectionCompass? WindDirectionCompass { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a number of degrees.
        /// </summary>
        [DataMember(Name = "windDirectionDegrees")]
        public WeatherMeasuring WindDirectionDegrees { get; set; }

        /// <summary>
        /// WindSpeed
        /// </summary>
        [DataMember(Name = "windSpeed")]
        public WeatherMeasuring WindSpeed { get; set; }

        /// <summary>
        /// States the strength of the wind during the survey event.
        /// </summary>
        [DataMember(Name = "windStrength")]
        public WindStrength? WindStrength { get; set; }

        /// <summary>
        /// States the precipitation conditions during the survey event.
        /// </summary>
        [DataMember(Name = "precipitation")]
        public Precipitation? Precipitation { get; set; }

        /// <summary>
        /// States the visibility conditions during the survey event.
        /// </summary>
        [DataMember(Name = "visibility")]
        public Visibility? Visibility { get; set; }

        /// <summary>
        /// States the cloud condtions during the survey event.
        /// </summary>
        [DataMember(Name = "cloudiness")]
        public Cloudiness? Cloudiness { get; set; }
    }
}
