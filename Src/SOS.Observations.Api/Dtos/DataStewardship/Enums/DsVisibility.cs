using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// States the visibility conditions during the survey event.
    /// </summary>
    public enum DsVisibility
    {
        /// <summary>
        /// dimma, <1 km
        /// </summary>
        [EnumMember(Value = "dimma, <1 km")]
        Dimma1Km = 0,
        /// <summary>
        /// dis, 1-4 km
        /// </summary>
        [EnumMember(Value = "dis, 1-4 km")]
        Dis1Till4Km = 1,
        /// <summary>
        /// god, 10-20 km
        /// </summary>
        [EnumMember(Value = "god, 10-20 km")]
        God10Till20Km = 2,
        /// <summary>
        /// mycket god, >20 km
        /// </summary>
        [EnumMember(Value = "mycket god, >20 km")]
        MycketGod20Km = 3,
        /// <summary>
        /// måttlig, 4-10 km
        /// </summary>
        [EnumMember(Value = "måttlig, 4-10 km")]
        Måttlig4Till10Km = 4
    }
}

