namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Summary of active and inactive SOS observation processing.</summary>
public class ProcessSummaryDto
{
    public ProcessStatusDto? ActiveProcessStatus { get; set; }

    public ProcessStatusDto? InactiveProcessStatus { get; set; }

    public List<DataProviderStatusDto>? DataProviderStatuses { get; set; }
}


