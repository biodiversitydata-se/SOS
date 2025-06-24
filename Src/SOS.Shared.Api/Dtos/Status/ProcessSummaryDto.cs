namespace SOS.Shared.Api.Dtos.Status;
public class ProcessSummaryDto
{
    public ProcessStatusDto ActiveProcessStatus { get; set; }
    public ProcessStatusDto InactiveProcessStatus { get; set; }
    public List<DataProviderStatusDto> DataProviderStatuses { get; set; }
}