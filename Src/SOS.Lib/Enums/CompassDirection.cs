using System.Runtime.Serialization;

namespace SOS.Lib.Enums
{
    /// <summary>
    /// States the wind direction during the survey event as a compass direction.
    /// </summary>
    public enum CompassDirection
    {
        /// <summary>
        /// nord
        /// </summary>
        [EnumMember(Value = "nord")]
        North = 0,
        /// <summary>
        /// nordost
        /// </summary>
        [EnumMember(Value = "nordost")]
        Northeast = 1,
        /// <summary>
        /// nordväst
        /// </summary>
        [EnumMember(Value = "nordväst")]
        Northwest = 2,
        /// <summary>
        /// ost
        /// </summary>
        [EnumMember(Value = "ost")]
        East = 3,
        /// <summary>
        /// syd
        /// </summary>
        [EnumMember(Value = "syd")]
        South = 4,
        /// <summary>
        /// sydost
        /// </summary>
        [EnumMember(Value = "sydost")]
        Southeast = 5,
        /// <summary>
        /// sydväst
        /// </summary>
        [EnumMember(Value = "sydväst")]
        Southwest = 6,
        /// <summary>
        /// väst
        /// </summary>
        [EnumMember(Value = "väst")]
        West = 7
    }
}
