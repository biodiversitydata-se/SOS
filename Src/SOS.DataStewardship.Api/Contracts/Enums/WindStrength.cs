using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Enums
{
    /// <summary>
    /// States the strength of the wind during the survey event.
    /// </summary>
    public enum WindStrength
    {
        /// <summary>
        ///  0 Beaufort
        /// </summary>
        [EnumMember(Value = "0 Beaufort")]
        _0Beaufort = 0,
        /// <summary>
        ///  1 Beaufort
        /// </summary>
        [EnumMember(Value = "1 Beaufort")]
        _1Beaufort = 1,
        /// <summary>
        ///  2 Beaufort
        /// </summary>
        [EnumMember(Value = "2 Beaufort")]
        _2Beaufort = 2,
        /// <summary>
        ///  3 Beaufort
        /// </summary>
        [EnumMember(Value = "3 Beaufort")]
        _3Beaufort = 3,
        /// <summary>
        ///  4 Beaufort
        /// </summary>
        [EnumMember(Value = "4 Beaufort")]
        _4Beaufort = 4,
        /// <summary>
        ///  5 Beaufort
        /// </summary>
        [EnumMember(Value = "5 Beaufort")]
        _5Beaufort = 5,
        /// <summary>
        ///  6 Beaufort
        /// </summary>
        [EnumMember(Value = "6 Beaufort")]
        _6Beaufort = 6,
        /// <summary>
        ///  7 Beaufort
        /// </summary>
        [EnumMember(Value = "7 Beaufort")]
        _7Beaufort = 7,
        /// <summary>
        ///  8 Beaufort
        /// </summary>
        [EnumMember(Value = "8 Beaufort")]
        _8Beaufort = 8,
        /// <summary>
        ///  9 Beaufort
        /// </summary>
        [EnumMember(Value = "9 Beaufort")]
        _9Beaufort = 9,
        /// <summary>
        ///  10 Beaufort
        /// </summary>
        [EnumMember(Value = "10 Beaufort")]
        _10Beaufort = 10,
        /// <summary>
        ///  10 Beaufort
        /// </summary>
        [EnumMember(Value = "11 Beaufort")]
        _11Beaufort = 11,
        /// <summary>
        ///  10 Beaufort
        /// </summary>
        [EnumMember(Value = "12 Beaufort")]
        _12Beaufort = 12,
        /// <summary>
        ///  vindstilla, <1 m/s
        /// </summary>
        [EnumMember(Value = "vindstilla, <1 m/s")]
        Vindstilla1Ms = 100,
        /// <summary>
        ///  svag vind, 1-3 m/s
        /// </summary>
        [EnumMember(Value = "svag vind, 1-3 m/s")]
        SvagVind1Till3Ms = 101,
        /// <summary>
        ///  måttlig vind, 4-7 m/s
        /// </summary>
        [EnumMember(Value = "måttlig vind, 4-7 m/s")]
        MåttligVind4Till7Ms = 102,
        /// <summary>
        ///  frisk vind, 8-13 m/s
        /// </summary>
        [EnumMember(Value = "frisk vind, 8-13 m/s")]
        FriskVind8Till13Ms = 103,
        /// <summary>
        ///  hård vind, 14-19 m/s
        /// </summary>
        [EnumMember(Value = "hård vind, 14-19 m/s")]
        HårdVind14Till19Ms = 104,
        /// <summary>
        ///  mycket hård vind, 20-24 m/s
        /// </summary>
        [EnumMember(Value = "mycket hård vind, 20-24 m/s")]
        MycketHårdVind20Till24Ms = 105,
        /// <summary>
        ///  storm, 25-32 m/s
        /// </summary>
        [EnumMember(Value = "storm, 25-32 m/s")]
        Storm25Till32Ms = 106,
        /// <summary>
        ///  orkan, ≥33 m/s
        /// </summary>
        [EnumMember(Value = "orkan, ≥33 m/s")]
        Orkan33Ms = 107
    }
}
