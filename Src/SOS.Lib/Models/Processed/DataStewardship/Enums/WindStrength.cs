using System.Runtime.Serialization;

namespace SOS.Lib.Models.Processed.DataStewardship.Enums
{
    /// <summary>
    /// States the strength of the wind during the survey event.
    /// </summary>
    public enum WindStrength
    {
        /// <summary>
        ///  vindstilla, mindre än 1 m/s
        /// </summary>
        [EnumMember(Value = "vindstilla, <1 m/s")]
        Vindstilla1Ms = 0,
        /// <summary>
        ///  svag vind, 1-3 m/s
        /// </summary>
        [EnumMember(Value = "svag vind, 1-3 m/s")]
        SvagVind1Till3Ms = 1,
        /// <summary>
        ///  måttlig vind, 4-7 m/s
        /// </summary>
        [EnumMember(Value = "måttlig vind, 4-7 m/s")]
        MåttligVind4Till7Ms = 2,
        /// <summary>
        ///  frisk vind, 8-13 m/s
        /// </summary>
        [EnumMember(Value = "frisk vind, 8-13 m/s")]
        FriskVind8Till13Ms = 3,
        /// <summary>
        ///  hård vind, 14-19 m/s
        /// </summary>
        [EnumMember(Value = "hård vind, 14-19 m/s")]
        HårdVind14Till19Ms = 4,
        /// <summary>
        ///  mycket hård vind, 20-24 m/s
        /// </summary>
        [EnumMember(Value = "mycket hård vind, 20-24 m/s")]
        MycketHårdVind20Till24Ms = 5,
        /// <summary>
        ///  storm, 25-32 m/s
        /// </summary>
        [EnumMember(Value = "storm, 25-32 m/s")]
        Storm25Till32Ms = 6,
        /// <summary>
        ///  orkan, ≥33 m/s
        /// </summary>
        [EnumMember(Value = "orkan, ≥33 m/s")]
        Orkan33Ms = 7,
        /// <summary>
        ///  0 Beaufort
        /// </summary>
        [EnumMember(Value = "0 Beaufort")]
        _0Beaufort = 8,
        /// <summary>
        ///  1 Beaufort
        /// </summary>
        [EnumMember(Value = "1 Beaufort")]
        _1Beaufort = 9,
        /// <summary>
        ///  2 Beaufort
        /// </summary>
        [EnumMember(Value = "2 Beaufort")]
        _2Beaufort = 10,
        /// <summary>
        ///  3 Beaufort
        /// </summary>
        [EnumMember(Value = "3 Beaufort")]
        _3Beaufort = 11,
        /// <summary>
        ///  4 Beaufort
        /// </summary>
        [EnumMember(Value = "4 Beaufort")]
        _4Beaufort = 12,
        /// <summary>
        ///  5 Beaufort
        /// </summary>
        [EnumMember(Value = "5 Beaufort")]
        _5Beaufort = 13,
        /// <summary>
        ///  6 Beaufort
        /// </summary>
        [EnumMember(Value = "6 Beaufort")]
        _6Beaufort = 14,
        /// <summary>
        ///  7 Beaufort
        /// </summary>
        [EnumMember(Value = "7 Beaufort")]
        _7Beaufort = 15,
        /// <summary>
        ///  8 Beaufort
        /// </summary>
        [EnumMember(Value = "8 Beaufort")]
        _8Beaufort = 16,
        /// <summary>
        ///  9 Beaufort
        /// </summary>
        [EnumMember(Value = "9 Beaufort")]
        _9Beaufort = 17,
        /// <summary>
        ///  10 Beaufort
        /// </summary>
        [EnumMember(Value = "10 Beaufort")]
        _10Beaufort = 18
    }
}
