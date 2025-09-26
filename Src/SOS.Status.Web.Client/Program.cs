using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.HttpClients;
using SOS.Status.Web.Client.Models;
using SOS.Status.Web.Client.Models.BlazorSamples;
using System.Globalization;

// WebAssembly startup
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//builder.Services.AddScoped(sp =>
//    new HttpClient { BaseAddress = new Uri("http://localhost:5006") });

builder.Services.AddScoped<IStatusInfoService, StatusInfoApiClient>();
builder.Services.AddScoped<ITaxonDiagramService, TaxonDiagramApiClient>();
builder.Services.AddScoped<IObservationSearchService, ObservationsSearchApiClient>();
builder.Services.AddScoped<AppState>();
builder.Services.AddSingleton<IAppEnvironment>(sp => new AppEnvironment(builder.HostEnvironment.Environment));

// Authentication and Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Local storage
builder.Services.AddBlazoredLocalStorage();

// Use Swedish culture
var culture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();