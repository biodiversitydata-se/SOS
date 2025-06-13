using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SOS.Shared.Api.Dtos.Status;
using System.Globalization;
namespace SOS.Status.Web.Components.UI.Datasets;
public partial class DataProviderIndexStatus
{
    [Parameter]
    public ProcessStatusDto? ProcessStatus { get; set; }

    [Parameter]
    public List<DataProviderStatusDto>? DataProviderStatuses { get; set; }

    [Parameter]
    public bool ActiveInstance { get; set; }

    [Inject]
    private ILogger<DataProviderIndexStatus> Logger { get; set; }

    protected string GetProcessStatusCircleStyle(string status)
    {
        if (status == null)
            return "color:orange; font-size:18px;";
        if (status.Equals("success", StringComparison.InvariantCultureIgnoreCase))
            return "color:green; font-size:18px;";
        else
            return "color:red; font-size:18px;";
    }
}