using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Enums
{
    /// <summary>
    /// States the quality of a species observation, i.e. whether its verified by an expert or similar. Quality categories are chosen from a codelist.
    /// </summary>
    public enum IdentificationVerificationStatus
    {
        /// <summary>
        /// värdelista saknas
        /// </summary>
        [EnumMember(Value = "värdelista saknas")]
        VärdelistaSaknas = 0
    }

}
