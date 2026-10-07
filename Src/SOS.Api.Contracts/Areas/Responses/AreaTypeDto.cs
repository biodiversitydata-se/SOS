using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Areas.Responses;
/// <summary>
/// Area type dto
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AreaTypeDto>))]
public enum AreaTypeDto
{

    /// <summary>A municipality (kommun).</summary>
    Municipality = 1,

    /// <summary>A community area.</summary>
    Community = 11,

    /// <summary>A sea area.</summary>
    Sea = 12,

    /// <summary>A region within a country.</summary>
    CountryRegion = 13,

    /// <summary>An area classified by nature type.</summary>
    NatureType = 15,

    /// <summary>A historical province (landskap).</summary>
    Province = 16,

    /// <summary>A Ramsar wetland area.</summary>
    Ramsar = 17,

    /// <summary>An area used for bird-observation validation.</summary>
    BirdValidationArea = 18,

    /// <summary>A parish (församling).</summary>
    Parish = 19,

    /// <summary>A Special Protection Area (SPA).</summary>
    Spa = 20,

    /// <summary>A county (län).</summary>
    County = 21,

    /// <summary>A protected nature area.</summary>
    ProtectedNature = 22,

    /// <summary>A Swedish Forest Agency district.</summary>
    SwedishForestAgencyDistricts = 24,

    /// <summary>A Site of Community Importance (SCI).</summary>
    Sci = 26,

    /// <summary>A water area.</summary>
    WaterArea = 27,

    /// <summary>A 5 × 5 km atlas grid cell.</summary>
    Atlas5x5 = 29,

    /// <summary>A 10 × 10 km atlas grid cell.</summary>
    Atlas10x10 = 30,

    /// <summary>An SFV district.</summary>
    SfvDistricts = 34,

    /// <summary>A campus area.</summary>
    Campus = 35,

    /// <summary>An EU biogeographical region.</summary>
    EuBiogeographicalRegions = 38,

    /// <summary>An EU marine region.</summary>
    EuMarineRegions = 39,

}


