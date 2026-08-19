# SOS API Client

`SOS.Api.Client` is a .NET client for SOS Observations and Analysis API. It includes typed clients and higher-level services for observation search, lookups, areas with geometry, exports, data stewardship datasets, API status and analysis.

## Registration

```csharp
builder.Services.AddSosWebClients(options =>
{
    options.ObservationsApiBaseAddress = new Uri("https://sos.example/");
    options.AnalysisApiBaseAddress = new Uri("https://sos-analysis.example/");
    options.ApplicationIdentifier = "my-application";
    options.SystemName = "My application";
});
```

`AddSosWebClients` relays the current ASP.NET Core access token. Use `AddSosClients` when authentication is supplied by a custom HTTP handler.

## Using the client

Consumers can inject a specific raw client or service, or use `SosClient` as a single entry point:

```csharp
public sealed class ObservationReader(SosClient sos)
{
    public Task<Observation?> GetAsync(string occurrenceId, CancellationToken cancellationToken) =>
        sos.Observations.GetObservationByIdInternalAsync(
            occurrenceId,
            cancellationToken: cancellationToken);
}
```

Raw clients represent individual SOS endpoints. Services compose reusable workflows such as sensitive-observation detection, paged export jobs and workspace filter creation. Raw clients can also be constructed directly with an appropriately configured `HttpClient` for tests or non-DI scenarios.

The observation client includes both public and internal observation-by-id methods. Adaptive search returns the response body together with the SOS result headers; deserialize JSON bodies to `AdaptiveSearchResultDto<Observation>` when `OutputFormatDto.Json` is selected. `ISosLookupService.GetAreaWithGeometryAsync` handles the zipped area export and caches the geometry.

## Selected role

An optional `ISosRoleIdProvider` supplies the selected role for each outgoing request:

```csharp
public sealed class CurrentSosRoleIdProvider(IHttpContextAccessor accessor)
    : ISosRoleIdProvider
{
    public int? GetCurrentRoleId() =>
        accessor.HttpContext?.Items["SelectedRoleId"] is int value
            ? value
            : null;
}

builder.Services.AddSingleton<ISosRoleIdProvider, CurrentSosRoleIdProvider>();
```

Register at most one provider. It must read the current role for every call and must not store request-specific state. An explicit role on a client call takes precedence over the provider. Application and system identifiers come from `SosClientOptions`.

## Workspace filters

`ISosWorkspaceFilterService` converts the persistence-independent `SosWorkspaceSearchDefinition` to SOS filters:

```csharp
var workspace = new SosWorkspaceSearchDefinition
{
    DataProviderIds = [1, 2],
    DateFilter = new() { Mode = WorkspaceDateFilterMode.LastFiveYears },
    ObservationDetails = new() { Presence = WorkspacePresence.ObservedOnly }
};

var filter = await sos.WorkspaceFilters.CreateSearchFilterAsync(
    workspace,
    cancellationToken: cancellationToken);
var result = await sos.Observations.ObservationsBySearchInternalAsync(
    filter,
    cancellationToken: cancellationToken);
```

Persistence, sharing and user settings remain in the consuming application. Explicit taxon identifiers require no adapter. If a workspace expands descendants, filters expanded taxa by category, or uses taxon or species-group lists, register exactly one application data source:

```csharp
builder.Services.AddTransient<ISosWorkspaceTaxonSource, MyWorkspaceTaxonSource>();
```

Invalid workspace settings produce `SosWorkspaceFilterValidationException`.

## Downloads and errors

Observation aggregation, analysis and export clients use the configurable `LongRunningTimeout` (240 seconds by default). Analysis and export POST order operations are never retried, avoiding duplicate jobs, while safe transient failures can use the configured resilience policy. Export downloads return `SosStreamingResponse` and must be disposed:

```csharp
await using var download = await sos.Exports.DownloadInternalAsync(
    filter,
    SosExportType.Csv,
    cancellationToken: cancellationToken);
await download.Content.CopyToAsync(destination, cancellationToken);
```

Unsuccessful HTTP responses throw `SosApiException`. A successful response with an invalid or unexpectedly empty body throws `SosInvalidResponseException`; endpoints where `204 No Content` is valid return `null`.

## HTTP resilience

`AddSosClients` and `AddSosWebClients` enable retry with jitter, request and attempt timeouts, circuit breaking and rate limiting. Analysis and export order POST requests are excluded from retries.

Use either the built-in pipeline or an application-owned pipeline. Disable the built-in pipeline if `configureBuilder` adds another resilience handler:

```csharp
builder.Services.AddSosClients(
    options =>
    {
        options.ObservationsApiBaseAddress = new Uri("https://sos.example/");
        options.AnalysisApiBaseAddress = new Uri("https://sos-analysis.example/");
    },
    useResilience: false,
    configureBuilder: client => client.AddStandardResilienceHandler());
```

`configureBuilder` can add authentication, logging or other non-resilience handlers while the built-in pipeline remains enabled.

The package targets .NET 10. `ArtData.SosClient` is a separate legacy client and continues to coexist with this package.
