using SOS.Api.Contracts.Status.Responses;

namespace SOS.Status.Web.Client.Models;

public class ProcessSummaryViewModel
{
    public ProcessStatusDto ActiveProcessStatus { get; set; }
    public ProcessStatusDto InactiveProcessStatus { get; set; }
    public List<DataProviderStatusViewModel> DataProviderStatuses { get; set; }
}
