namespace SOS.Status.Web.Client.Dtos;

public class ProcessSummaryDto
{
    public ProcessStatusDto ActiveProcessStatus { get; set; }
    public ProcessStatusDto InactiveProcessStatus { get; set; }
    public List<DataProviderStatusDto> DataProviderStatuses { get; set; }
}