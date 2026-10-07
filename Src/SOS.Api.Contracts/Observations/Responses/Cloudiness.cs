using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// States the cloud conditions during the survey event.
/// </summary>
public enum Cloudiness
{
    /// <summary>Halvklart, 3-5/8.</summary>
    [EnumMember(Value = "halvklart, 3-5/8")]
    PartlyClear3To5Av8 = 0,

    /// <summary>Klart, 0/8.</summary>
    [EnumMember(Value = "klart, 0/8")]
    Clear0Of8 = 1,

    /// <summary>Molnigt, 6-7/8.</summary>
    [EnumMember(Value = "molnigt, 6-7/8")]
    Cloudy6To7Of8 = 2,

    /// <summary>Mulet, 8/8.</summary>
    [EnumMember(Value = "mulet, 8/8")]
    Overcast8Of8 = 3,

    /// <summary>Nästan klart, 1-2/8.</summary>
    [EnumMember(Value = "nästan klart, 1-2/8")]
    AlmostClear1To2Av8 = 4,

    /// <summary>Växlande, 0-8/8.</summary>
    [EnumMember(Value = "växlande, 0-8/8")]
    EverChanging0Till8Av8 = 5,
}


