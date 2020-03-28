using MongoDB.Driver.GeoJsonObjectModel;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    /// Site object
    /// </summary>
    public class Site
    {
        private PolygonGeoShape _pointWithBuffer;
        private PointGeoShape _point;

        /// <summary>
        /// Accuracy in meters
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// County of site
        /// </summary>
        public GeographicalArea County { get; set; }

        /// <summary>
        /// Countyry part of site
        /// </summary>
        public GeographicalArea CountryPart { get; set; }

        /// <summary>
        /// Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public GeographicalArea Municipality { get; set; }

        /// <summary>
        /// Parish
        /// </summary>
        public GeographicalArea Parish { get; set; }

        /// <summary>
        /// Point (WGS84)
        /// </summary>
        public PointGeoShape Point
        {
            get
            {
                if (_point == null)
                {
                    InitGeometries();
                }

                return _point;
            }
            set => _point = value;
        }

        /// <summary>
        /// Point with accuracy buffer (WGS84)
        /// </summary>
        public PolygonGeoShape PointWithBuffer
        {
            get
            {
                if (_pointWithBuffer == null)
                {
                    InitGeometries();
                }

                return _pointWithBuffer;
            }
            set => _pointWithBuffer = value;
        }

        private void InitGeometries()
        {
            if (XCoord > 0 && YCoord > 0)
            {
                var webMercatorPoint = new Point(XCoord, YCoord);
                var wgs84Point = (Point)webMercatorPoint.Transform(VerbatimCoordinateSystem, CoordinateSys.WGS84);
                _point = (PointGeoShape)wgs84Point?.ToGeoShape();
                _pointWithBuffer = (PolygonGeoShape) wgs84Point?.ToCircle(Accuracy)?.ToGeoShape();
            }
        }
        
        /// <summary>
        /// Province
        /// </summary>
        public GeographicalArea Province { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// X coordinate of site
        /// </summary>
        public int XCoord { get; set; }


        /// <summary>
        /// Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }

        public CoordinateSys VerbatimCoordinateSystem { get; set; }
    }
}
