using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>States the snow conditions on the ground during the survey event.</summary>
public enum SnowCover
{
    /// <summary>Barmark.</summary>
    [EnumMember(Value = "barmark")]
    SnowFreeGround = 0,

    /// <summary>Snötäckt mark.</summary>
    [EnumMember(Value = "snötäckt mark")]
    SnowCoveredGround = 1,

    /// <summary>Mycket tunt eller fläckvis snötäcke.</summary>
    [EnumMember(Value = "mycket tunt snötäcke eller fläckvis snötäcke")]
    ThinOrPartialSnowCoveredGround = 2,
}


