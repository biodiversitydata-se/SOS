using Microsoft.AspNetCore.Components;
using SOS.Shared.Api.Dtos.Status;

namespace SOS.Status.Web.Components.UI.Datasets;
public partial class DataProviderGridVerbose
{
    [Parameter]
    public ProcessSummaryDto? ProcessSummary { get; set; }

    [Parameter]
    public bool Sortable { get; set; } = false;
}