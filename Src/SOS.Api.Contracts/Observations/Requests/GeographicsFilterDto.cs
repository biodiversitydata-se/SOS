using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Geometry filter.
/// </summary>
public class GeographicsFilterDto
{

    /// <summary>
    /// Area filter
    /// </summary>
    public ICollection<AreaFilterDto>? Areas { get; set; }

    public LatLonBoundingBoxDto? BoundingBox { get; set; }

    /// <summary>
    /// If true, observations that are outside Geometries polygons
    /// <br/>but close enough when disturbance sensitivity of species
    /// <br/>are considered, will be included in the result.
    /// </summary>
    public bool ConsiderDisturbanceRadius { get; set; }

    /// <summary>
    /// If true, observations that are outside Geometries polygons
    /// <br/>but possibly inside when accuracy (coordinateUncertaintyInMeters)
    /// <br/>of observation is considered, will be included in the result.
    /// </summary>
    public bool ConsiderObservationAccuracy { get; set; }

    /// <summary>
    /// Use the authorization area buffer when searching sensitive observations.
    /// </summary>
    public bool? ConsiderAuthorizationBuffer { get; set; }

    /// <summary>
    /// Controls whether observations outside Sweden are included.
    /// </summary>
    public bool? IncludeObservationsOutsideSweden { get; set; }

    /// <summary>
    /// If Geometries is of point type, this property must be set to a value greater than 0.
    /// <br/>Observations inside circle (center=point, radius=MaxDistanceFromPoint) will be returned.
    /// </summary>
    public double? MaxDistanceFromPoint { get; set; }

    /// <summary>
    /// Point or polygon geometry used for search.
    /// <br/>If the geometry is a point, then MaxDistanceFromPoint is also used in search.
    /// </summary>
    public ICollection<GeoJSON.Text.Geometry.IGeometryObject>? Geometries { get; set; }

    /// <summary>
    /// Filter on location id/s. Only observations with passed location id/s this will be returned
    /// </summary>
    public ICollection<string>? LocationIds { get; set; }

    /// <summary>
    /// Location name wild card filter
    /// </summary>
    public string? LocationNameFilter { get; set; }

    /// <summary>
    /// Limit observation accuracy. Only observations with accuracy less than this will be returned
    /// </summary>
    public int? MaxAccuracy { get; set; }
    
}


