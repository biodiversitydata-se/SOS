using Microsoft.AspNetCore.Components;
using SOS.Status.Web.Client.Dtos;
namespace SOS.Status.Web.Client.UI.DataProviders;

public partial class DataProviderIndexStatus
{
    [Parameter]
    public ProcessStatusDto? ProcessStatus { get; set; }

    [Parameter]
    public List<DataProviderStatusDto>? DataProviderStatuses { get; set; }

    [Parameter]
    public bool ActiveInstance { get; set; }

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