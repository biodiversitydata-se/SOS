using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    public enum WindDirectionCompass
    {
        /// <summary>
        /// nord
        /// </summary>
        [EnumMember(Value = "nord")]
        Nord = 0,
        /// <summary>
        /// nordost
        /// </summary>
        [EnumMember(Value = "nordost")]
        Nordost = 1,
        /// <summary>
        /// nordväst
        /// </summary>
        [EnumMember(Value = "nordväst")]
        Nordväst = 2,
        /// <summary>
        /// ost
        /// </summary>
        [EnumMember(Value = "ost")]
        Ost = 3,
        /// <summary>
        /// syd
        /// </summary>
        [EnumMember(Value = "syd")]
        Syd = 4,
        /// <summary>
        /// sydost
        /// </summary>
        [EnumMember(Value = "sydost")]
        Sydost = 5,
        /// <summary>
        /// sydväst
        /// </summary>
        [EnumMember(Value = "sydväst")]
        Sydväst = 6,
        /// <summary>
        /// väst
        /// </summary>
        [EnumMember(Value = "väst")]
        Väst = 7
    }
}
