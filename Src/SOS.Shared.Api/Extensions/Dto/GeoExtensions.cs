using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Gis;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Extensions.Dto;

public static class GeoExtensions
{
    public static GeoGridTileTaxonPageResultDto ToGeoGridTileTaxonPageResultDto(this GeoGridTileTaxonPageResult pageResult)
    {
        return new GeoGridTileTaxonPageResultDto
        {
            NextGeoTilePage = pageResult.NextGeoTilePage,
            NextTaxonIdPage = pageResult.NextTaxonIdPage,
            HasMorePages = pageResult.HasMorePages,
            GridCells = pageResult.GridCells.Select(m => m.ToGeoGridTileTaxaCellDto())
        };
    }

    public static GeoGridTileTaxaCellDto ToGeoGridTileTaxaCellDto(this GeoGridTileTaxaCell cell)
    {
        return new GeoGridTileTaxaCellDto
        {
            BoundingBox = ToLatLonBoundingBoxDto(cell.BoundingBox),
            GeoTile = cell.GeoTile,
            X = cell.X,
            Y = cell.Y,
            Zoom = cell.Zoom,
            Taxa = cell.Taxa.Select(m => new GeoGridTileTaxonObservationCountDto
            {
                TaxonId = m.TaxonId,
                ObservationCount = m.ObservationCount
            })
        };
    }

    public static FeatureCollection ToGeoJson(this GeoGridMetricResultDto geoGridMetricResult, MetricCoordinateSys metricCoordinateSys)
    {
        var featureCollection = new FeatureCollection
        {
            BoundingBox = new Envelope(
                geoGridMetricResult.BoundingBox.BottomRight.Longitude,
                geoGridMetricResult.BoundingBox.TopLeft.Longitude,
                geoGridMetricResult.BoundingBox.BottomRight.Latitude,
                geoGridMetricResult.BoundingBox.TopLeft.Latitude)
        };

        foreach (var gridCell in geoGridMetricResult.GridCells)
        {                
            var polygon = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.BottomRight.Y),
                    new Coordinate(gridCell.MetricBoundingBox.BottomRight.X, gridCell.MetricBoundingBox.BottomRight.Y),
                    new Coordinate(gridCell.MetricBoundingBox.BottomRight.X, gridCell.MetricBoundingBox.TopLeft.Y),
                    new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.TopLeft.Y),
                    new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.BottomRight.Y)
                }));

            var wgs84Polygon = polygon.Transform((CoordinateSys)metricCoordinateSys, CoordinateSys.WGS84);
            
            var feature = new Feature
            {                    
                Attributes = new AttributesTable(new[]
                {
                    new KeyValuePair<string, object>("id", gridCell.Id),
                    new KeyValuePair<string, object>("cellSizeInMeters", geoGridMetricResult.GridCellSizeInMeters),
                    new KeyValuePair<string, object>("observationsCount", gridCell.ObservationsCount ?? 0),
                    new KeyValuePair<string, object>("taxaCount", gridCell.TaxaCount ?? 0),
                    new KeyValuePair<string, object>("metricCRS", metricCoordinateSys.ToString()),
                    new KeyValuePair<string, object>("metricLeft", Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.X)),
                    new KeyValuePair<string, object>("metricTop", Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.Y)),
                    new KeyValuePair<string, object>("metricRight", Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.X)),
                    new KeyValuePair<string, object>("metricBottom", Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.Y))
                }),                    
                Geometry = wgs84Polygon,
                BoundingBox = wgs84Polygon.EnvelopeInternal
            };
            featureCollection.Add(feature);
        }
       
        return featureCollection;
    }

    /// <summary>
    /// Cast lat lon bounding box dto 
    /// </summary>
    /// <param name="latLonBoundingBox"></param>
    /// <returns></returns>
    public static LatLonBoundingBox ToLatLonBoundingBox(this LatLonBoundingBoxDto latLonBoundingBox)
    {
        if (latLonBoundingBox?.TopLeft == null || latLonBoundingBox?.BottomRight == null)
        {
            return null!;
        }

        return new LatLonBoundingBox
        {
            TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinate(),
            BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinate()
        };
    }

    /// <summary>
    /// Cast dto Coordinate
    /// </summary>
    /// <param name="latLonCoordinate"></param>
    /// <returns></returns>
    public static LatLonCoordinate ToLatLonCoordinate(this LatLonCoordinateDto latLonCoordinate)
    {
        if (latLonCoordinate == null)
        {
            return null!;
        }

        return new LatLonCoordinate(latLonCoordinate.Latitude, latLonCoordinate.Longitude);
    }

    public static LatLonBoundingBoxDto ToLatLonBoundingBoxDto(this LatLonBoundingBox latLonBoundingBox)
    {
        return new LatLonBoundingBoxDto
        {
            TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinateDto(),
            BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinateDto()
        };
    }

    public static LatLonBoundingBoxDto ToLatLonBoundingBoxDto(this XYBoundingBox xyBoundingBox)
    {
        return new LatLonBoundingBoxDto
        {
            TopLeft = xyBoundingBox.TopLeft.ToLatLonCoordinateDto(),
            BottomRight = xyBoundingBox.BottomRight.ToLatLonCoordinateDto()
        };
    }

    public static XYBoundingBoxDto ToXYBoundingBoxDto(this LatLonBoundingBox latLonBoundingBox)
    {
        return new XYBoundingBoxDto
        {
            TopLeft = latLonBoundingBox.TopLeft.ToXYCoordinateDto(),
            BottomRight = latLonBoundingBox.BottomRight.ToXYCoordinateDto()
        };
    }

    public static LatLonCoordinateDto ToLatLonCoordinateDto(this LatLonCoordinate latLonCoordinate)
    {
        return new LatLonCoordinateDto
        {
            Latitude = latLonCoordinate.Latitude,
            Longitude = latLonCoordinate.Longitude
        };
    }

    public static LatLonCoordinateDto ToLatLonCoordinateDto(this XYCoordinate xyCoordinate)
    {
        var point = new Point(xyCoordinate.X, xyCoordinate.Y).Transform(CoordinateSys.SWEREF99_TM,
            CoordinateSys.WGS84, false) as Point;
        return new LatLonCoordinateDto
        {
            Latitude = point.Y,
            Longitude = point.X
        };
    }

    public static XYCoordinateDto ToXYCoordinateDto(this LatLonCoordinate latLonCoordinate)
    {
        var point = new Point(latLonCoordinate.Longitude, latLonCoordinate.Latitude).Transform(CoordinateSys.WGS84,
            CoordinateSys.SWEREF99_TM, false) as Point;
        return new XYCoordinateDto
        {
            X = point.X,
            Y = point.Y
        };
    }

    public static XYBoundingBoxDto ToXYBoundingBoxDto(this XYBoundingBox xyBoundingBox)
    {
        return new XYBoundingBoxDto
        {
            BottomRight = xyBoundingBox.BottomRight.ToXYCoordinateDto(),
            TopLeft = xyBoundingBox.TopLeft.ToXYCoordinateDto()
        };
    }

    public static XYCoordinateDto ToXYCoordinateDto(this XYCoordinate xyCoordinate)
    {
        return new XYCoordinateDto
        {
            X = xyCoordinate.X,
            Y = xyCoordinate.Y
        };
    }

    public static GeoGridCellDto ToGeoGridCellDto(this GridCellTile gridCellTile)
    {
        return new GeoGridCellDto
        {
            X = gridCellTile.X,
            Y = gridCellTile.Y,
            TaxaCount = gridCellTile.TaxaCount,
            ObservationsCount = gridCellTile.ObservationsCount,
            Zoom = gridCellTile.Zoom,
            BoundingBox = gridCellTile.BoundingBox.ToLatLonBoundingBoxDto()
        };
    }

    public static GridCellDto ToGridCellDto(this GridCell gridCell, int gridCellSizeInMeters)
    {
        return new GridCellDto
        {
            Id = GeoJsonHelper.GetGridCellId(gridCellSizeInMeters, Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.X), Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.Y)),
            BoundingBox = gridCell.MetricBoundingBox.ToLatLonBoundingBoxDto(),
            ObservationsCount = gridCell.ObservationsCount,
            MetricBoundingBox = gridCell.MetricBoundingBox.ToXYBoundingBoxDto(),
            TaxaCount = gridCell.TaxaCount
        };
    }
}