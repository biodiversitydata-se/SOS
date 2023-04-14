using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// States the snow conditions on the ground during the survey event.
    /// </summary>
    public enum SnowCover
    {
        /// <summary>
        /// barmark
        /// </summary>
        [EnumMember(Value = "barmark")]
        Barmark = 0,
        /// <summary>
        /// snötäckt mark
        /// </summary>
        [EnumMember(Value = "snötäckt mark")]
        SnötäcktMark = 1,
        /// <summary>
        /// mycket tunt snötäcke eller fläckvis snötäcke
        /// </summary>
        [EnumMember(Value = "mycket tunt snötäcke eller fläckvis snötäcke")]
        MycketTuntSnötäckeEllerFläckvisSnötäcke = 2
    }
}
