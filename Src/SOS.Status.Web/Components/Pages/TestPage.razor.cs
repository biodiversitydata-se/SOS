using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SOS.Status.Web.Components.UI.Test;

namespace SOS.Status.Web.Components.Pages;
public partial class TestPage
{
    private ElementReference? inputElement;
    private int increment = 0;
    private string searchText = string.Empty;
    private string? myString = "Initial value";
    private bool showDismissableAlert = true;
    private DismissableAlert? dismissableAlert;
    private string? outerPos;
    private string? innerPos;

    //private List<LocationItem> points = null; // Testa ErrorBoundary
    private List<LocationItem> points = new()
    {
        new LocationItem { Id = 1, Name = "Uppsala", Latitude = 59.8586, Longitude = 17.6389 },
        new LocationItem { Id = 2, Name = "Göteborg", Latitude = 57.7089, Longitude = 11.9746 },
        new LocationItem { Id = 3, Name = "Luleå", Latitude = 65.5848, Longitude = 22.1567 }
    };

    public string Style { get; set; } = "color:red";

    [Parameter]
    [SupplyParameterFromQuery]
    public int? Id { get; set; }

    private void ToggleDismissableAlert()
    {
        showDismissableAlert = !showDismissableAlert;
    }

    private async Task ChangeNameAsync(string? name)
    {
        State.SetUsername(name);
        await protectedLocalStorage.SetAsync("myNameKey", State.Username!);
        Logger.LogInformation("Name changed to {Name}", name);
    }
    
    private async Task TestJavaScriptAsync()
    {
        await JsRuntime.InvokeVoidAsync("console.log", "Hej från Blazor!");
        await JsRuntime.InvokeVoidAsync("alert", "Hej från Blazor!");
    }

    private async Task PerformSearch()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        searchText = $"Hittade resultat för {searchText}";
    }

    private void OuterMouseMove(MouseEventArgs e)
    {
        outerPos = $"Yttre: {e.ClientX}, {e.ClientY}";
    }

    private void InnerMouseMove(MouseEventArgs e)
    {
        innerPos = $"Inre: {e.ClientX}, {e.ClientY}";
    }

    private void MyStateHasChanged()
    {
        Console.WriteLine(State.Username);
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        var result = await protectedLocalStorage.GetAsync<string>("myNameKey");
        State.SetUsername(result.Success ? result.Value : null);
        //State.OnChange += StateHasChanged;
        State.OnChange += MyStateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (inputElement is not null)
            {
                await JsRuntime.InvokeVoidAsync("blazorFocus.set", inputElement);
            }
        }
    }

    public class LocationItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}