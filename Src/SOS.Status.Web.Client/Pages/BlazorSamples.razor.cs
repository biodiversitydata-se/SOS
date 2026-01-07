using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SOS.Status.Web.Client.UI.BlazorSamples;
using System.Reflection;

namespace SOS.Status.Web.Client.Pages;

public partial class BlazorSamples
{
    private ElementReference? inputElement;
    private int increment = 0;
    private string searchText = string.Empty;
    private string? myString = "Initial value";
    private bool showDismissableAlert = true;
    private DismissableAlert? dismissableAlert;
    private string? outerPos;
    private string? innerPos;
    private string markdown { get; } = "text *italics* and **bold**\r\n# Heading1\r\nText\r\n### Heading3\r\n- Test";
    private string diagramDef = def2;
    private IJSObjectReference jsModule;

    [PersistentState]
    public int CurrentCountWithPersistence { get; set; }

    [PersistentState]
    public int ActiveTabIndex {  get; set; }
    public int CurrentCount { get; set; }

    const string def1 = @"
        flowchart LR
        A --> B
        B --> C
        C --> A
        ";

    const string def2 = @"
mindmap
Responsibilities
  HR
    Salaries
    Healthcare
  IT
    Workstations
    Phones
  Sales
    Sell, sell, sell!
  Operations
    Operate
  Marketing
    Branding
    Website
";

    //private List<LocationItem> points = null; // Test ErrorBoundary
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
        await LocalStorage.SetItemAsync("myNameKey", State.Username!);
        Logger.LogInformation("Name changed to {Name}", name);
    }

    private async Task TestJavaScriptAsync()
    {
        //await JsRuntime.InvokeVoidAsync("blazorFocus.showMessage", "Hello!");
        if (jsModule is not null)
        {
            await jsModule.InvokeVoidAsync("showMessage", "Hello!");
        }
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
        Logger.LogInformation("MyStateHasChanged() triggered. Username is {State}", State.Username);
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {                              
        State.OnChange += MyStateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (jsModule is null)
        {
            jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/BlazorSamples.razor.js");            
        }

        if (firstRender)
        {           
            string? result = await LocalStorage.GetItemAsync<string>("myNameKey");
            State.SetUsername(result);

            if (inputElement is not null && jsModule is not null)
            {
                //await JsRuntime.InvokeVoidAsync("blazorFocus.set", inputElement);
                await jsModule.InvokeVoidAsync("set", inputElement);
            }
        }
    }

    private void IncrementCount()
    {
        CurrentCountWithPersistence++;
        CurrentCount++;
    }

    private async Task PauseCircuitAsync()
    {
        await jsModule!.InvokeVoidAsync("pauseCircuit");
    }

    public class LocationItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}