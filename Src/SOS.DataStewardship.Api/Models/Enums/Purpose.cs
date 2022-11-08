using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models.Enums
{
    /// <summary>
    /// The purpose of the data collection (e.g. national or regional environmental monitoring).
    /// </summary>
    public enum Purpose
    {
        /// <summary>
        /// Nationell miljöövervakning
        /// </summary>
        [EnumMember(Value = "nationell miljöövervakning")]
        NationellMiljöövervakning = 0,
        /// <summary>
        /// Regional miljöövervakning
        /// </summary>
        [EnumMember(Value = "regional miljöövervakning")]
        RegionalMiljöövervakning = 1,
        /// <summary>
        /// Biogeografisk uppföljning
        /// </summary>
        [EnumMember(Value = "biogeografisk uppföljning")]
        biogeografiskUppföljning = 2
    }
}
