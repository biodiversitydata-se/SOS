using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Client.Requests;

/// <summary>Calculation options for area of occupancy and extent of occurrence.</summary>
public class AooEooOptions : SosRoleRequestOptions
{
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool? ValidateFilter { get; set; }
    /// <summary>Alpha values used to calculate the alpha hull.</summary>
    public ICollection<double>? AlphaValues { get; set; }
    /// <summary>The grid-cell side length, in metres.</summary>
    public int? GridCellSizeInMeters { get; set; } = 2000;
    /// <summary>
    /// Controls whether each occupied grid cell is represented by its centre point. When false, the grid
    /// cell's corner points are used.
    /// </summary>
    public bool? UseCenterPoint { get; set; } = true;
    /// <summary>
    /// Controls how <see cref="AlphaValues"/> are interpreted. When true, each value is a fraction of the
    /// difference between the longest and shortest Delaunay edges; otherwise it is a maximum edge length.
    /// </summary>
    public bool? UseEdgeLengthRatio { get; set; } = true;
    /// <summary>Controls whether holes are allowed in the calculated area.</summary>
    public bool? AllowHoles { get; set; } = false;
    /// <summary>Controls whether grid cells are included in the result.</summary>
    public bool? ReturnGridCells { get; set; } = false;
    /// <summary>Controls whether empty grid cells are included in the result.</summary>
    public bool? IncludeEmptyCells { get; set; } = false;
    /// <summary>The metric coordinate system used for calculations.</summary>
    public MetricCoordinateSys? MetricCoordinateSystem { get; set; } = MetricCoordinateSys.SWEREF99_TM;
    /// <summary>The coordinate system used for the returned geometry.</summary>
    public CoordinateSys? CoordinateSystem { get; set; } = CoordinateSys.WGS84;
}
