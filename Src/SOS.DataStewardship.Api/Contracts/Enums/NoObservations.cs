using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Enums
{
    /// <summary>
    /// States whether any of the sought after organisms were observed during the survey event or not. \"Sant\" (i.e. True) means that none of the sought after organisms were observed at all.
    /// </summary>
    public enum NoObservations
    {
        /// <summary>
        /// falskt
        /// </summary>
        [EnumMember(Value = "falskt")]
        Falskt = 0,
        /// <summary>
        /// sant
        /// </summary>
        [EnumMember(Value = "sant")]
        Sant = 1
    }
}
