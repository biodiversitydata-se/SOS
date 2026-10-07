using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.Search.Input;

/// <summary>
/// Server-side form of the geographic API input. The published contract stays GeoJSON-based.
/// </summary>
public sealed class GeographicsFilterInputModel : GeographicsFilterDto
{
    [JsonIgnore]
    public List<Geometry>? ServerGeometries { get; set; }

    /// <summary>Retains a failed GeoJSON conversion so validation can reject the input.</summary>
    [JsonIgnore]
    public string? GeometryConversionError { get; private init; }

    /// <summary>Converts GeoJSON-based input to server geometries, or reuses existing server input.</summary>
    /// <remarks>A missing DTO creates empty server input.</remarks>
    public static GeographicsFilterInputModel FromDto(GeographicsFilterDto? source)
    {
        if (source is GeographicsFilterInputModel input) return input;

        var geometries = source.TryGetSearchGeometries();
        input = new GeographicsFilterInputModel
        {
            ServerGeometries = geometries.IsSuccess ? geometries.Value : null,
            GeometryConversionError = geometries.IsFailure ? geometries.Error : null
        };
        if (source is not null) CopyProperties(source, input);
        return input;
    }

    public GeographicsFilterDto ToSerializableGeographicsFilterDto()
    {
        var dto = new GeographicsFilterDto
        {
            Geometries = this.GetSearchGeometries()?.Select(geometry => geometry.ToGeoJsonGeometry()).ToList()
        };
        CopyProperties(this, dto);
        return dto;
    }

    private static void CopyProperties(GeographicsFilterDto source, GeographicsFilterDto target)
    {
        target.Areas = source.Areas;
        target.BoundingBox = source.BoundingBox;
        target.ConsiderDisturbanceRadius = source.ConsiderDisturbanceRadius;
        target.ConsiderObservationAccuracy = source.ConsiderObservationAccuracy;
        target.ConsiderAuthorizationBuffer = source.ConsiderAuthorizationBuffer;
        target.IncludeObservationsOutsideSweden = source.IncludeObservationsOutsideSweden;
        target.MaxDistanceFromPoint = source.MaxDistanceFromPoint;
        target.LocationIds = source.LocationIds;
        target.LocationNameFilter = source.LocationNameFilter;
        target.MaxAccuracy = source.MaxAccuracy;
    }
}
