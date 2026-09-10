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

## Internal search request logging

Logging of internal search request parameters and bodies is disabled by default. Enable it explicitly by selecting a log level when registering the clients:

```csharp
builder.Services.AddSosClients(options =>
{
    options.ObservationsApiBaseAddress = new Uri("https://sos.example/");
    options.AnalysisApiBaseAddress = new Uri("https://sos-analysis.example/");
    options.RequestLoggingLevel = LogLevel.Debug;
    options.MaxLoggedRequestBodyLength = 1024;
});
```

The configured logging provider must also enable that level for the client categories. For example:

```json
{
  "Logging": {
    "LogLevel": {
      "SOS.Api.Client.Clients": "Debug"
    }
  }
}
```

Each internal observation-filter request is logged once with its HTTP method, full URI and request body. Encryption-password query parameters are always redacted. Bodies exceeding the configured character limit first have large taxon-id and coordinate arrays replaced with placeholders and are then strictly truncated if necessary. Request logging can expose filter values and should only be enabled where the resulting log data is appropriate.

## Clients and services

The library has two API layers:

- Clients map directly to SOS endpoints. Use them when one API call is sufficient.
- Services compose calls or add behavior such as sensitive-observation checks, GeoJSON parsing, area caching, export paging and workspace filter creation.

`SosClient` is a convenient single entry point to both layers. For example, `Observations` is a raw endpoint client:

```csharp
public sealed class ObservationReader(SosClient sos)
{
    public Task<Observation?> GetAsync(string occurrenceId, CancellationToken cancellationToken) =>
        sos.Observations.GetObservationByIdInternalAsync(
            occurrenceId,
            cancellationToken: cancellationToken);
}
```

For a narrower dependency, inject the required client or service directly. This service performs a sensitive-only count without changing the supplied filter:

```csharp
public sealed class SensitiveObservationChecker(
    ISosObservationSearchService observationSearch)
{
    public Task<bool> HasSensitiveAsync(
        SearchFilterInternalDto filter,
        CancellationToken cancellationToken) =>
        observationSearch.HasSensitiveObservationsAsync(
            filter,
            cancellationToken: cancellationToken);
}
```

The same service is available as `sos.ObservationSearch`. Prefer direct injection when a class only needs one client or service; use `SosClient` when a workflow naturally uses several. Raw clients can also be constructed directly with an appropriately configured `HttpClient` for tests or non-DI scenarios.

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

`ISosWorkspaceFilterService`, also available as `sos.WorkspaceFilters`, accepts a complete `SosWorkspaceDocument` from `SOS.Api.Contracts.Workspaces`:

```csharp
using SOS.Api.Contracts.Workspaces;

var workspace = new SosWorkspaceDocument
{
    Name = "Observations this year",
    Search = new()
    {
        DataProviderIds = [1, 3],
        DateFilter = new() { Mode = WorkspaceDateFilterMode.CurrentYear },
        ObservationDetails = new() { Presence = WorkspacePresence.ObservedOnly }
    }
};

var filter = await sos.WorkspaceFilters.CreateSearchFilterAsync(
    workspace,
    cancellationToken: cancellationToken);
var result = await sos.Observations.ObservationsBySearchInternalAsync(
    filter,
    cancellationToken: cancellationToken);
```

Use `CreateAggregationFilterAsync(workspace, ...)` for aggregation and analysis workflows. Both methods also accept `workspace.Search` directly when only a search definition is available. The document overloads validate `SchemaVersion` and require a non-null `Search` before converting. They interpret only the search definition and leave saved metadata, settings and application state untouched. Apply saved analysis/export SRID preferences to the corresponding operation options after validating that the operation supports them.

Pass `WorkspaceFilterOverrides` for request-specific output fields, bounding boxes, point/radius searches, date limits or replacement taxon IDs. These values are applied to the generated filter without updating the saved workspace. Storage and sharing remain in the consuming application or a user-data service. A workspace document can be stored with `JsonSerializer.Serialize` and restored with `JsonSerializer.Deserialize<SosWorkspaceDocument>` using the contracts package alone.

Explicit taxon identifiers require no taxon-source adapter. If a workspace expands descendants, filters expanded taxa by category, or uses taxon or species-group lists, register exactly one application data source:

```csharp
builder.Services.AddTransient<ISosWorkspaceTaxonSource, MyWorkspaceTaxonSource>();
```

Use the document helpers in `SOS.Api.Client.Workspaces` to validate before saving and read application state without handling `JsonElement`:

```csharp
SosWorkspaceDocuments.Validate(workspace); // Local checks; no taxon API calls.
workspace.SetApplicationState("my-app", new MapState { Zoom = 8 });
var json = SosWorkspaceDocuments.Serialize(workspace);
var restored = SosWorkspaceDocuments.Deserialize(json);
var map = restored.GetApplicationState<MapState>("my-app");
// MapState is your application's own type.
```

`Validate` checks the schema, search presence, supported selection enums/area types and date ordering/year boundaries. It does not resolve taxa, check authorization or perform full server-side filter validation. Conversion also checks computed date bounds against the current clock.

`Deserialize` checks the version before interpreting search fields. Missing/unsupported versions, malformed JSON and invalid search data produce `SosWorkspaceFilterValidationException`. Direct `JsonSerializer` use still throws `JsonException` for malformed JSON. There is no automatic schema migration.

Application-state helpers preserve other applications' entries, serialize a snapshot on write and return an independent value on read. Missing entries return default; incompatible state throws `JsonException`. Optional serializer options apply only to that application's state. Unknown root metadata (`ExtensionData`) is preserved by document serialization.

Workspace filter semantics:

- Missing or empty `DataProviderIds` selects no providers; set the desired provider IDs explicitly.
- `TaxonSelection = null` applies no taxon restriction. An empty selection, or a selection resolving to no taxa, matches no taxa.
- Included taxa/species groups are intersected with selected taxon lists. A selected source resolving to an empty set stays empty; it never removes a restriction. Taxon-source implementations must throw on lookup failures instead of returning an empty set.
- Relative dates are evaluated in the Swedish time zone when the filter is created. The existing `LastFiveYears` mode spans the current and five preceding calendar years; `LastTwentyFiveYears` spans the current and twenty-five preceding calendar years. Those persisted values retain their original meaning. New selections can use `LastFiveCalendarYears` or `LastTwentyFiveCalendarYears` for exactly five or twenty-five years, including the current year. Older clients reject these new enum values; upgrade readers before saving them.
- Date overrides intersect the computed interval in every date mode; reversed or non-overlapping intervals throw a validation exception. Observation and report date modes retain their respective end-of-day and midnight boundaries.
- For recurring month/day periods (`IgnoreYear`), date overrides require both bounds within one calendar year. The intersection must be one contiguous period, and saved year restrictions still apply. Disjoint intersections must be requested separately. The resulting filter uses an ordinary absolute interval. Without overrides, periods crossing New Year remain valid; leap-day periods use a leap-year anchor when necessary.
- A point override is added to active workspace geometries. `TaxonIds` overrides replace the workspace taxon selection for that request.

The search definition and its persistent selection types now live in `SOS.Api.Contracts.Workspaces`. Update imports and rebuild existing consumers when upgrading. `WorkspaceFilterOverrides` and `SosResolvedTaxon` remain in `SOS.Api.Client.Workspaces`. Legacy BFF database JSON needs an explicit adapter to the new document format, including loading the separately stored workspace taxa.

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

With `useResilience: true` (the default), the SOS client owns resilience for its HTTP clients. Previously registered resilience handlers are removed and replaced by one client-specific SOS pipeline. Register application-wide HTTP client defaults, such as Aspire service defaults, before calling `AddSosClients` or `AddSosWebClients`.

Use `useResilience: false` when the application owns resilience. In that mode the package neither removes nor adds resilience handlers, so a pipeline registered through application-wide defaults or `configureBuilder` remains in place:

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
