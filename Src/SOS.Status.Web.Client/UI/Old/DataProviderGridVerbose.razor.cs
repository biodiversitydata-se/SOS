using Microsoft.AspNetCore.Components;
using SOS.Status.Web.Client.Dtos;

namespace SOS.Status.Web.Client.UI.Old;

public partial class DataProviderGridVerbose
{
    [Parameter]
    public ProcessSummaryDto? ProcessSummary { get; set; }

    [Parameter]
    public bool Sortable { get; set; } = false;
}