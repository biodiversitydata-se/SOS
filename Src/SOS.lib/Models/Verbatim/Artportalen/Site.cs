using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    ///     Site object
    /// </summary>
    public class Site
    {
        private GeoJsonGeometry _point;
        private GeoJsonGeometry _pointWithBuffer;

        /// <summary>
        ///     Accuracy in meters
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        ///     County of site
        /// </summary>
        public GeographicalArea County { get; set; }

        /// <summary>
        ///     Countyry part of site
        /// </summary>
        public GeographicalArea CountryPart { get; set; }

        /// <summary>
        ///     Country Region
        /// </summary>
        public GeographicalArea CountryRegion { get; set; }

        /// <summary>
        ///     Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Name of site
        /// </summary>
        public GeographicalArea Municipality { get; set; }

        /// <summary>
        ///     Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Parish
        /// </summary>
        public GeographicalArea Parish { get; set; }

        /// <summary>
        ///     Presentation name Parish
        /// </summary>
        public string PresentationNameParishRegion { get; set; }

        /// <summary>
        ///     Point (WGS84)
        /// </summary>
        public GeoJsonGeometry Point
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
        ///     Point with accuracy buffer (WGS84)
        /// </summary>
        public GeoJsonGeometry PointWithBuffer
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

        /// <summary>
        ///     Protected Nature
        /// </summary>
        public GeographicalArea ProtectedNature { get; set; }

        /// <summary>
        ///     Province
        /// </summary>
        public GeographicalArea Province { get; set; }


        /// <summary>
        ///     Special Protection Area, Natura 2000, Birds Directive
        /// </summary>
        public GeographicalArea SpecialProtectionArea { get; set; }

        /// <summary>
        ///     X coordinate of site
        /// </summary>
        public int XCoord { get; set; }


        /// <summary>
        ///     Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }

        public CoordinateSys VerbatimCoordinateSystem { get; set; }

        /// <summary>
        ///     Water Area
        /// </summary>
        public GeographicalArea WaterArea { get; set; }

        /// <summary>
        ///     The parent site id
        /// </summary>
        public int? ParentSiteId { get; set; }

        /// <summary>
        ///     Name of the parent site
        /// </summary>
        public string ParentSiteName { get; set; }

        private void InitGeometries()
        {
            if (XCoord > 0 && YCoord > 0)
            {
                var webMercatorPoint = new Point(XCoord, YCoord);
                var wgs84Point = (Point) webMercatorPoint.Transform(VerbatimCoordinateSystem, CoordinateSys.WGS84);
                _point = wgs84Point?.ToGeoJson();
                _pointWithBuffer = wgs84Point?.ToCircle(Accuracy)?.ToGeoJson();
            }
        }
    }
}