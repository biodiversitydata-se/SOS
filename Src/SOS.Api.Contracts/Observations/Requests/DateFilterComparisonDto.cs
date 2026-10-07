namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Selects which observation date components are compared with a month or year filter.</summary>
public enum DateFilterComparisonDto
{

    /// <summary>Compare the observation's start date.</summary>
    StartDate = 0,

    /// <summary>Compare the observation's end date.</summary>
    EndDate = 1,

    /// <summary>Require both the start and end date to match.</summary>
    BothStartDateAndEndDate = 2,

    /// <summary>Match months occurring anywhere between the observation's start and end date. Used with month filters.</summary>
    StartDateEndDateMonthRange = 3,

}


