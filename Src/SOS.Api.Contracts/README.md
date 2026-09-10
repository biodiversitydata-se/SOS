# SOS API Contracts

`SOS.Api.Contracts` contains shared SOS contracts: request, response, GeoJSON and enum types for the Observations and Analysis APIs, and portable workspace documents.

Install this package when an application needs the API contracts or stores and exchanges workspaces. Applications that call the SOS APIs should normally install `SOS.Api.Client`, which references this package automatically.

The contracts follow the JSON API and do not expose the server's `SOS.Lib` assembly.

## Workspace documents

`SOS.Api.Contracts.Workspaces.SosWorkspaceDocument` is the common format for saving and exchanging a workspace. It contains:

- `SchemaVersion`: the document format version, independent of the NuGet version. The current version is `1`.
- `Name`: the display name.
- `Search`: a `SosWorkspaceSearchDefinition` containing the user's search selections, including explicit taxa, list references and relative date modes.
- `Settings`: optional analysis and export SRID preferences. These are operation preferences, not search restrictions.
- `ApplicationState`: optional JSON state keyed by application identifier. An app updates its own entry and preserves other entries.

Database identifiers, ownership, access control and sharing belong to the application or user-data service that stores the document. Such a service can reference this package without referencing `SOS.Api.Client`.

```csharp
using SOS.Api.Contracts.Workspaces;
using System.Text.Json;

var workspace = new SosWorkspaceDocument
{
    Name = "Bird observations this year",
    Search = new()
    {
        DataProviderIds = [1, 3],
        DateFilter = new() { Mode = WorkspaceDateFilterMode.CurrentYear },
        TaxonSelection = new()
        {
            Taxa = [new() { TaxonId = 4000104, IncludeChildren = true }]
        }
    },
    Settings = new() { AnalysisSrid = 3006, ExportSrid = 4326 },
    ApplicationState = new()
    {
        ["my-application"] = JsonSerializer.SerializeToElement(new { selectedTab = "map" })
    }
};

var json = JsonSerializer.Serialize(workspace);
var restored = JsonSerializer.Deserialize<SosWorkspaceDocument>(json)!;
```

Save `Search` as user selections. The client resolves relative dates and taxon selections when creating an API filter. Expanding child taxa or lists requires a taxon source in the client application. All active search geometries use WGS84 GeoJSON; labels, inactive features and application-specific file references can be kept in `ApplicationState`.

Applications using `SOS.Api.Client` can use `SosWorkspaceDocuments.Deserialize`, `Serialize` and `Validate` for local validation, and `GetApplicationState<T>`/`SetApplicationState` for typed application state. These helpers live in `SOS.Api.Client.Workspaces`; the contracts package remains independent of the client.

## JSON compatibility

Workspace properties have explicit camel-case JSON names, and workspace enums are written as strings with `System.Text.Json`. No application-specific serializer configuration is needed. GeoJSON geometry types and coordinates survive serialization and deserialization.

`schemaVersion` and `search` must be present when deserializing a document. Existing BFF JSON is not implicitly treated as this format. Before interpreting a document, consumers must check the schema version and ensure `Search` is not null; the client's document overloads perform these checks.

Unknown document metadata and settings are preserved through `JsonExtensionData`. Unknown search properties, including properties in nested search selections, are rejected with `JsonException`, so an unsupported restriction cannot silently disappear. Application state is opaque JSON and is preserved. Only load, edit and execute document schema versions your application supports; use an explicit migration when changing the schema's meaning.

## Moving from the client workspace types

The search definition and its persistent selection types have moved from `SOS.Api.Client.Workspaces` to `SOS.Api.Contracts.Workspaces`. This changes their namespace and assembly. Update imports and rebuild consumers with compatible versions of both packages. `WorkspaceFilterOverrides` and `SosResolvedTaxon` remain in the client namespace because they describe request-time behavior and taxon resolution.

This is a new document format, not a replacement database entity for the copied fynddata BFF. When adapting that application, load its separate `WorkspaceTaxon` rows into `Search.TaxonSelection`, map only selected geometries into `Search.Geometries`, and retain inactive features and other UI state separately. Map the existing property names and enum values explicitly rather than deserializing old workspace JSON directly into these types.
