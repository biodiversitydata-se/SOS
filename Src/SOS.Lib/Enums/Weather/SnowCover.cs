using System.Runtime.Serialization;

namespace SOS.Lib.Enums.Weather
{
    public enum SnowCover
    {
        /// <summary>
        /// barmark
        /// </summary>
        [EnumMember(Value = "barmark")]
        SnowFreeGround = 0,
        /// <summary>
        /// snötäckt mark
        /// </summary>
        [EnumMember(Value = "snötäckt mark")]
        SnowCoveredGround = 1,
        /// <summary>
        /// mycket tunt snötäcke eller fläckvis snötäcke
        /// </summary>
        [EnumMember(Value = "mycket tunt snötäcke eller fläckvis snötäcke")]
        ThinOrPartialSnowCoveredGround = 2
    }
}
