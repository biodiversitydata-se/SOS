using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// The type of species observation the record deals with (e.g. human observation, material sample etc.)
    /// </summary>
    public enum BasisOfRecord
    {
        /// <summary>
        /// fysiskt prov
        /// </summary>
        [EnumMember(Value = "fysiskt prov")]
        FysisktProv = 0,
        /// <summary>
        /// maskinell observation
        /// </summary>
        [EnumMember(Value = "maskinell observation")]
        MaskinellObservation = 1,
        /// <summary>
        /// mänsklig observation
        /// </summary>
        [EnumMember(Value = "mänsklig observation")]
        MänskligObservation = 2,
        /// <summary>
        /// okänt
        /// </summary>
        [EnumMember(Value = "okänt")]
        Okänt = 3
    }
}