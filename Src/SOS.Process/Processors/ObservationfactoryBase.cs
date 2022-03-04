using System.Globalization;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Process.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ObservationFactoryBase
    {
        /// <summary>
        /// Init location class
        /// </summary>
        /// <param name="location"></param>
        /// <param name="verbatimLongitude"></param>
        /// <param name="verbatimLatitude"></param>
        /// <param name="verbatimCoordinateSystem"></param>
        /// <param name="point"></param>
        /// <param name="pointWithBuffer"></param>
        /// <param name="pointWithDisturbanceBuffer"></param>
        /// <param name="coordinateUncertaintyInMeters"></param>
        private void InitializeLocation(Location location, double? verbatimLongitude, double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, Point point, PolygonGeoShape pointWithBuffer, PolygonGeoShape pointWithDisturbanceBuffer, int? coordinateUncertaintyInMeters)
        {
            location.Continent = new VocabularyValue { Id = (int)ContinentId.Europe };
            location.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters;
            location.Country = new VocabularyValue { Id = (int)CountryId.Sweden };
            location.CountryCode = Lib.Models.DarwinCore.Vocabulary.CountryCode.Sweden;
            location.GeodeticDatum = CoordinateSys.WGS84.EpsgCode();
           
            if (point == null)
            {
                return;
            }

            location.DecimalLongitude = point.X;
            location.DecimalLatitude = point.Y;
            location.Point = point.ToGeoShape() as PointGeoShape;
            location.PointLocation = point.ToGeoLocation();
            location.PointWithBuffer = pointWithBuffer;
            location.PointWithDisturbanceBuffer = pointWithDisturbanceBuffer;

            var sweRef99TmPoint = point.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
            location.Sweref99TmX = sweRef99TmPoint.Coordinate.X;
            location.Sweref99TmY = sweRef99TmPoint.Coordinate.Y;

            location.VerbatimSRS = verbatimCoordinateSystem.EpsgCode();

            if (!verbatimLatitude.HasValue || !verbatimLongitude.HasValue)
            {
                return;
            }
            location.VerbatimLatitude = verbatimLatitude.Value.ToString(CultureInfo.InvariantCulture);
            location.VerbatimLongitude = verbatimLongitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get point with disturbance buffer (if any)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="taxonDisturbanceRadius"></param>
        /// <returns></returns>
        private Geometry GetPointWithDisturbanceBuffer(Point point, int? taxonDisturbanceRadius)
        {
            if (!(taxonDisturbanceRadius.HasValue && taxonDisturbanceRadius.Value > 0))
            {
                return null;
            }

            return point.ToCircle(taxonDisturbanceRadius);
        }

        /// <summary>
        /// Add position data
        /// </summary>
        /// <param name="location"></param>
        /// <param name="verbatimLongitude"></param>
        /// <param name="verbatimLatitude"></param>
        /// <param name="verbatimCoordinateSystem"></param>
        /// <param name="coordinateUncertaintyInMeters"></param>
        /// <param name="taxonDisturbanceRadius"></param>
        protected void AddPositionData(Location location, double? verbatimLongitude, double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
        {
            Point point = null;
            if (verbatimLongitude.HasValue && verbatimLongitude.Value > 0 && verbatimLatitude.HasValue && verbatimLatitude > 0)
            {
                point = new Point(verbatimLongitude.Value, verbatimLatitude.Value);

                if (verbatimCoordinateSystem != CoordinateSys.WGS84)
                {
                    point = point.Transform(verbatimCoordinateSystem, CoordinateSys.WGS84) as Point;
                }
            }

            var pointWithBuffer = point.ToCircle(coordinateUncertaintyInMeters);
            var pointWithDisturbanceBuffer = GetPointWithDisturbanceBuffer(point, taxonDisturbanceRadius);

            InitializeLocation(location, verbatimLongitude, verbatimLatitude, verbatimCoordinateSystem, point, pointWithBuffer?.ToGeoShape() as PolygonGeoShape, pointWithDisturbanceBuffer?.ToGeoShape() as PolygonGeoShape, coordinateUncertaintyInMeters);
        }

        /// <summary>
        /// Add position data
        /// </summary>
        /// <param name="location"></param>
        /// <param name="verbatimLongitude"></param>
        /// <param name="verbatimLatitude"></param>
        /// <param name="verbatimCoordinateSystem"></param>
        /// <param name="point"></param>
        /// <param name="pointWithBuffer"></param>
        /// <param name="coordinateUncertaintyInMeters"></param>
        /// <param name="taxonDisturbanceRadius"></param>
        protected void AddPositionData(Location location, double? verbatimLongitude,
            double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, Point point,
            GeoJsonGeometry pointWithBuffer, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
        {
            var pointWithDisturbanceBuffer = GetPointWithDisturbanceBuffer(point, taxonDisturbanceRadius);

            InitializeLocation(location, verbatimLongitude, verbatimLatitude, verbatimCoordinateSystem, point, pointWithBuffer?.ToGeoShape() as PolygonGeoShape, pointWithDisturbanceBuffer?.ToGeoShape() as PolygonGeoShape, coordinateUncertaintyInMeters);
        }

        /// <summary>
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="accessRightsId"></param>
        /// <returns></returns>
        protected int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon taxon)
        {
            return CalculateProtectionLevel(taxon, null);
        }

        /// <summary>
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="accessRightsId"></param>
        /// <returns></returns>
        protected int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon taxon, AccessRightsId? accessRightsId)
        {
            if (accessRightsId is AccessRightsId.FreeUsage) return 1;
            var protectionLevel = taxon?.Attributes?.ProtectionLevel?.Id ?? 1;
            return protectionLevel > 0 ? protectionLevel : 1;
        }

        protected VocabularyValue GetAccessRightsFromSensitivityCategory(int sensitivityCategory)
        {
            return VocabularyValue.Create((int)GetAccessRightsIdFromSensitivityCategory(sensitivityCategory));
        }

        private AccessRightsId GetAccessRightsIdFromSensitivityCategory(int sensitivityCategory)
        {
            if (sensitivityCategory > 1) return AccessRightsId.NotForPublicUsage;
            return AccessRightsId.FreeUsage;            
        }
    }
}
