using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Describes how much of a dataset is publicly available.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DsAccessRights>))]
public enum DsAccessRights
{
    /// <summary>The dataset is public.</summary>
    [EnumMember(Value = "publik")]
    Publik = 0,

    /// <summary>Access to the dataset is restricted.</summary>
    [EnumMember(Value = "begränsad")]
    Begränsad = 1,

    /// <summary>The dataset is not public.</summary>
    [EnumMember(Value = "ej offentlig")]
    EjOffentlig = 2
}

/// <summary>
/// Environmental monitoring programme area to which a dataset belongs.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DsProgrammeArea>))]
public enum DsProgrammeArea
{
    /// <summary>Biogeografisk uppföljning av naturtyper och arter.</summary>
    [EnumMember(Value = "Biogeografisk uppföljning av naturtyper och arter")]
    BiogeografiskUppföljningAvNaturtyperOchArter = 0,

    /// <summary>Fjäll.</summary>
    [EnumMember(Value = "Fjäll")]
    Fjäll = 1,

    /// <summary>Jordbruksmark.</summary>
    [EnumMember(Value = "Jordbruksmark")]
    Jordbruksmark = 2,

    /// <summary>Kust och hav.</summary>
    [EnumMember(Value = "Kust och hav")]
    KustOchHav = 3,

    /// <summary>Landskap.</summary>
    [EnumMember(Value = "Landskap")]
    Landskap = 4,

    /// <summary>Skog.</summary>
    [EnumMember(Value = "Skog")]
    Skog = 5,

    /// <summary>Våtmark.</summary>
    [EnumMember(Value = "Våtmark")]
    Våtmark = 6,

    /// <summary>Sjöar och vattendrag.</summary>
    [EnumMember(Value = "Sjöar och vattendrag")]
    SjöarOchVattendrag = 7,

    /// <summary>Fisk.</summary>
    [EnumMember(Value = "Fisk")]
    Fisk = 8
}

/// <summary>
/// Purpose of collecting the data in a dataset.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DsPurpose>))]
public enum DsPurpose
{
    /// <summary>Nationell miljöövervakning.</summary>
    [EnumMember(Value = "nationell miljöövervakning")]
    NationellMiljöövervakning = 0,

    /// <summary>Regional miljöövervakning.</summary>
    [EnumMember(Value = "regional miljöövervakning")]
    RegionalMiljöövervakning = 1,

    /// <summary>Biogeografisk uppföljning.</summary>
    [EnumMember(Value = "biogeografisk uppföljning")]
    BiogeografiskUppföljning = 2
}

/// <summary>
/// Type of project in which a dataset was collected.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DsProjectType>))]
public enum DsProjectType
{
    /// <summary>Artportalenprojekt.</summary>
    [EnumMember(Value = "artportalenprojekt")]
    Artportalenprojekt = 0,

    /// <summary>Delprogram.</summary>
    [EnumMember(Value = "delprogram")]
    Delprogram = 1,

    /// <summary>Delsystem.</summary>
    [EnumMember(Value = "delsystem")]
    Delsystem = 2,

    /// <summary>Gemensamt delprogram.</summary>
    [EnumMember(Value = "gemensamt delprogram")]
    GemensamtDelprogram = 3
}
