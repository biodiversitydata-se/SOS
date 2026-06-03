using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Dtos;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.Services.AdaptiveSearch;

public sealed class AdaptiveSearchRequest
{
    public int? RoleId { get; init; }
    public string? AuthorizationApplicationIdentifier { get; init; }
    public required SearchFilterInternalDto Filter { get; init; }
    public required LatLonBoundingBox? Bbox { get; init; }
    public required int UserId { get; init; }
    public required string TranslationCultureCode { get; init; }
    public int ObservationsLimit { get; init; }
    public int MaxZoom { get; init; }
    public int MaxGridCells { get; init; }
    public PropertyLabelType GeoJsonPropertyLabelType { get; init; }
    public OutputFormatDto OutputFormat { get; init; }
    public bool ExcludeNullValues { get; init; }
    public bool IncludeFilterAreasInResult { get; init; }
    public bool ReturnInputBBox { get; init; }
    public CreatePolygonsMode CreatePolygons { get; init; }
}

public sealed class AdaptiveSearchResult
{
    public required AdaptiveSearchResultKind Kind { get; init; }
    public object? JsonResult { get; init; }
    public string? GeoJson { get; init; }
    public AdaptiveSearchGeoJsonObservationsResult? GeoJsonObservations { get; init; }
    public required long ObservationsTotalCount { get; init; }
    public required int ResultCount { get; init; }
    public required string ResultType { get; init; }
    public int? Zoom { get; init; }
}

public enum AdaptiveSearchResultKind
{
    Json,
    GeoJsonGrid,
    GeoJsonObservations
}

public sealed class AdaptiveSearchGeoJsonObservationsResult
{
    public required IEnumerable<JsonObject> Records { get; init; }
    public required ICollection<string>? OutputFields { get; init; }
    public required bool FlatOut { get; init; }
    public required PropertyLabelType PropertyLabelType { get; init; }
    public required bool ExcludeNullValues { get; init; }
    public required LatLonBoundingBox? Bbox { get; init; }
    public required List<Feature>? GeographicAreas { get; init; }
    public required bool GroupByLocation { get; init; }
    public required Dictionary<string, Geometry>? PolygonLocationGeometries { get; init; }
}
