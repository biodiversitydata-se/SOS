namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Selects public observations, sensitive observations or both, subject to the caller's access rights.</summary>
public enum ProtectionFilterDto
{
    /// <summary>Public observations.</summary>
    Public = 0,

    /// <summary>Sensitive observations.</summary>
    Sensitive = 1,

    /// <summary>Both public and sensitive observations.</summary>
    BothPublicAndSensitive = 2,
}


