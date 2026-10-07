using Microsoft.AspNetCore.Components;
using SOS.Status.Web.Client.Models;

namespace SOS.Status.Web.Client.UI.Old;

public partial class DataProviderGridVerbose
{
    [Parameter]
    public ProcessSummaryViewModel? ProcessSummary { get; set; }

    [Parameter]
    public bool Sortable { get; set; } = false;
}
