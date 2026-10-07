using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Client.Requests;

/// <summary>Calculation options for Article 17 analysis.</summary>
public class Article17Options : SosRoleRequestOptions
{
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool? ValidateFilter { get; set; }
    /// <summary>The maximum distance between occurrence grid cells, in metres.</summary>
    public int? MaxDistance { get; set; } = 50000;
    /// <summary>The grid-cell side length, in metres.</summary>
    public int? GridCellSizeInMeters { get; set; } = 2000;
    /// <summary>The metric coordinate system used for calculations.</summary>
    public MetricCoordinateSys? MetricCoordinateSystem { get; set; } = MetricCoordinateSys.SWEREF99_TM;
    /// <summary>The coordinate system used for the returned geometry.</summary>
    public CoordinateSys? CoordinateSystem { get; set; } = CoordinateSys.WGS84;
}
