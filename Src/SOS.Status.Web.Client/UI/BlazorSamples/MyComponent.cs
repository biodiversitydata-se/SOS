using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SOS.Status.Web.Client.UI.BlazorSamples;

public class MyComponent : ComponentBase
{
    [Parameter]
    public string Message { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, $"Message: {Message}");
        builder.CloseElement();
    }
}