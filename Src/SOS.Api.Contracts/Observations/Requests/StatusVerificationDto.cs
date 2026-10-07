namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations by verification status.</summary>
public enum StatusVerificationDto
{

    /// <summary>Include both verified and unverified observations.</summary>
    BothVerifiedAndNotVerified = 0,

    /// <summary>Only include verified observations.</summary>
    Verified = 1,

    /// <summary>Only include unverified observations.</summary>
    NotVerified = 2,

}


