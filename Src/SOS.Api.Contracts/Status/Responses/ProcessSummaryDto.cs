namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Summary of active and inactive SOS observation processing.</summary>
public class ProcessSummaryDto
{
    /// <summary>The processing status of the active instance.</summary>
    public ProcessStatusDto? ActiveProcessStatus { get; set; }

    /// <summary>The processing status of the inactive instance.</summary>
    public ProcessStatusDto? InactiveProcessStatus { get; set; }

    /// <summary>Observation counts and harvesting and processing status for each provider.</summary>
    public List<DataProviderStatusDto>? DataProviderStatuses { get; set; }
}


