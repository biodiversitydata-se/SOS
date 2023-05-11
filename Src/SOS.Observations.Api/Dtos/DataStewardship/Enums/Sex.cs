using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// Sex of the observed organism.
    /// </summary>
    public enum Sex
    {
        /// <summary>
        /// hane
        /// </summary>
        [EnumMember(Value = "hane")]
        Hane = 0,
        /// <summary>
        /// hona
        /// </summary>
        [EnumMember(Value = "hona")]
        Hona = 1,
        /// <summary>
        /// i par
        /// </summary>
        [EnumMember(Value = "i par")]
        IPar = 2
    }
}
