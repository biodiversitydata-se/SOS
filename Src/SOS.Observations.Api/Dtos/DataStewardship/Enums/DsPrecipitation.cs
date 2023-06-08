using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// States the precipitation conditions during the survey event.
    /// </summary>
    public enum DsPrecipitation
    {
        /// <summary>
        ///  kraftigt regn
        /// </summary>
        [EnumMember(Value = "kraftigt regn")]
        KraftigtRegn = 0,
        /// <summary>
        ///  kraftigt snöfall
        /// </summary>
        [EnumMember(Value = "kraftigt snöfall")]
        KraftigtSnöfall = 1,
        /// <summary>
        ///  lätt regn
        /// </summary>
        [EnumMember(Value = "lätt regn")]
        LättRegn = 2,
        /// <summary>
        ///  lätt snöfall
        /// </summary>
        [EnumMember(Value = "lätt snöfall")]
        LättSnöfall = 3,
        /// <summary>
        ///  måttligt regn
        /// </summary>
        [EnumMember(Value = "måttligt regn")]
        MåttligtRegn = 4,
        /// <summary>
        ///  måttligt snöfall
        /// </summary>
        [EnumMember(Value = "måttligt snöfall")]
        MåttligtSnöfall = 5,
        /// <summary>
        ///  regnskurar
        /// </summary>
        [EnumMember(Value = "regnskurar")]
        Regnskurar = 6,
        /// <summary>
        ///  uppehåll
        /// </summary>
        [EnumMember(Value = "uppehåll")]
        Uppehåll = 7
    }
}
