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
    extension(GeoGridTileTaxonPageResult pageResult)
    {
        public GeoGridTileTaxonPageResultDto ToGeoGridTileTaxonPageResultDto()
        {
            return new GeoGridTileTaxonPageResultDto
            {
                NextGeoTilePage = pageResult.NextGeoTilePage,
                NextTaxonIdPage = pageResult.NextTaxonIdPage,
                HasMorePages = pageResult.HasMorePages,
                GridCells = pageResult.GridCells.Select(m => m.ToGeoGridTileTaxaCellDto())
            };
        }
    }

    extension(GeoGridTileTaxaCell cell)
    {
        public GeoGridTileTaxaCellDto ToGeoGridTileTaxaCellDto()
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
    }

    extension(GeoGridMetricResultDto geoGridMetricResult)
    {
        public FeatureCollection ToGeoJson(MetricCoordinateSys metricCoordinateSys)
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
    }

    extension(LatLonBoundingBoxDto latLonBoundingBox)
    {
        /// <summary>
        /// Cast lat lon bounding box dto 
        /// </summary>
        /// <returns></returns>
        public LatLonBoundingBox ToLatLonBoundingBox()
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
    }

    extension(LatLonCoordinateDto latLonCoordinate)
    {
        /// <summary>
        /// Cast dto Coordinate
        /// </summary>
        /// <returns></returns>
        public LatLonCoordinate ToLatLonCoordinate()
        {
            if (latLonCoordinate == null)
            {
                return null!;
            }

            return new LatLonCoordinate(latLonCoordinate.Latitude, latLonCoordinate.Longitude);
        }
    }

    extension(LatLonBoundingBox latLonBoundingBox)
    {
        public LatLonBoundingBoxDto ToLatLonBoundingBoxDto()
        {
            return new LatLonBoundingBoxDto
            {
                TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinateDto(),
                BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinateDto()
            };
        }

        public XYBoundingBoxDto ToXYBoundingBoxDto()
        {
            return new XYBoundingBoxDto
            {
                TopLeft = latLonBoundingBox.TopLeft.ToXYCoordinateDto(),
                BottomRight = latLonBoundingBox.BottomRight.ToXYCoordinateDto()
            };
        }
    }

    extension(XYBoundingBox xyBoundingBox)
    {
        public LatLonBoundingBoxDto ToLatLonBoundingBoxDto()
        {
            return new LatLonBoundingBoxDto
            {
                TopLeft = xyBoundingBox.TopLeft.ToLatLonCoordinateDto(),
                BottomRight = xyBoundingBox.BottomRight.ToLatLonCoordinateDto()
            };
        }

        public XYBoundingBoxDto ToXYBoundingBoxDto()
        {
            return new XYBoundingBoxDto
            {
                BottomRight = xyBoundingBox.BottomRight.ToXYCoordinateDto(),
                TopLeft = xyBoundingBox.TopLeft.ToXYCoordinateDto()
            };
        }
    }

    extension(LatLonCoordinate latLonCoordinate)
    {
        public LatLonCoordinateDto ToLatLonCoordinateDto()
        {
            return new LatLonCoordinateDto
            {
                Latitude = latLonCoordinate.Latitude,
                Longitude = latLonCoordinate.Longitude
            };
        }

        public XYCoordinateDto ToXYCoordinateDto()
        {
            var point = new Point(latLonCoordinate.Longitude, latLonCoordinate.Latitude).Transform(CoordinateSys.WGS84,
                CoordinateSys.SWEREF99_TM, false) as Point;
            return new XYCoordinateDto
            {
                X = point.X,
                Y = point.Y
            };
        }
    }

    extension(XYCoordinate xyCoordinate)
    {
        public LatLonCoordinateDto ToLatLonCoordinateDto()
        {
            var point = new Point(xyCoordinate.X, xyCoordinate.Y).Transform(CoordinateSys.SWEREF99_TM,
                CoordinateSys.WGS84, false) as Point;
            return new LatLonCoordinateDto
            {
                Latitude = point.Y,
                Longitude = point.X
            };
        }

        public XYCoordinateDto ToXYCoordinateDto()
        {
            return new XYCoordinateDto
            {
                X = xyCoordinate.X,
                Y = xyCoordinate.Y
            };
        }
    }

    extension(GridCellTile gridCellTile)
    {
        public GeoGridCellDto ToGeoGridCellDto()
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
    }

    extension(GridCell gridCell)
    {
        public GridCellDto ToGridCellDto(int gridCellSizeInMeters)
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
}