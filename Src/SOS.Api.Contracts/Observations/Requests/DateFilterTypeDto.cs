namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Determines how an observation's start and end dates are matched against an interval.</summary>
public enum DateFilterTypeDto
{
    /// <summary>The observation's start or end date must be within the interval.</summary>
    OverlappingStartDateAndEndDate = 0,

    /// <summary>The observation's start and end dates must both be within the interval.</summary>
    BetweenStartDateAndEndDate = 1,

    /// <summary>Only the observation's start date must be within the interval.</summary>
    OnlyStartDate = 2,

    /// <summary>Only the observation's end date must be within the interval.</summary>
    OnlyEndDate = 3,
}


