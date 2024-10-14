using System.Runtime.Serialization;

namespace SOS.Lib.Enums.Weather
{
    /// <summary>
    /// States the precipitation conditions during the survey event.
    /// </summary>
    public enum Precipitation
    {
        /// <summary>
        ///  uppehåll
        /// </summary>
        [EnumMember(Value = "uppehåll")]
        DryWeather = 0,
        /// <summary>
        ///  lätt regn
        /// </summary>
        [EnumMember(Value = "lätt regn")]
        LightRain = 1,
        /// <summary>
        ///  måttligt regn
        /// </summary>
        [EnumMember(Value = "måttligt regn")]
        ModerateRain = 2,
        /// <summary>
        ///  kraftigt regn
        /// </summary>
        [EnumMember(Value = "kraftigt regn")]
        HeavyRain = 3,
        /// <summary>
        ///  regnskurar
        /// </summary>
        [EnumMember(Value = "regnskurar")]
        Showers = 4,
        /// <summary>
        ///  lätt snöfall
        /// </summary>
        [EnumMember(Value = "lätt snöfall")]
        LightSnowfall = 11,
        /// <summary>
        ///  måttligt snöfall
        /// </summary>
        [EnumMember(Value = "måttligt snöfall")]
        ModerateSnowfall = 12,
        /// <summary>
        ///  kraftigt snöfall
        /// </summary>
        [EnumMember(Value = "kraftigt snöfall")]
        HeavySnowfall = 13,
        /// <summary>
        ///  snöbyar
        /// </summary>
        [EnumMember(Value = "snöbyar")]
        Snowflurries = 14,
        /// <summary>
        ///  hagelbyar Hail showers
        /// </summary>
        [EnumMember(Value = "hagelbyar")]
        HailShowers = 24
    }

}
