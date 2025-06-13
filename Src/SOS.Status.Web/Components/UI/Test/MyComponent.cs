using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SOS.Status.Web.Components.UI.Test;

public class MyComponent : ComponentBase
{
    [Parameter]
    public string Message { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, $"Meddelande: {Message}");
        builder.CloseElement();
    }
}