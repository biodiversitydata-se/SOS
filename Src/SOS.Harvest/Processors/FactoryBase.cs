using System.Globalization;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using SOS.Lib.Configuration.Process;
using MongoDB.Driver.GeoJsonObjectModel;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class FactoryBase
    {
        protected readonly ProcessConfiguration ProcessConfiguration;
        protected readonly DataProvider DataProvider;
        protected readonly IProcessTimeManager TimeManager;

        /// <summary>
        /// Get point with disturbance buffer (if any)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="taxonDisturbanceRadius"></param>
        /// <returns></returns>
        private Geometry GetPointWithDisturbanceBuffer(Point point, int? taxonDisturbanceRadius)
        {
            if ((taxonDisturbanceRadius ?? 0) <= 0)
            {
                return null!;
            }

            return point.ToCircle(taxonDisturbanceRadius!.Value);
        }

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
        private void InitializeLocation(Location location, double? verbatimLongitude, double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, Point point, PolygonGeoShape? pointWithBuffer, PolygonGeoShape? pointWithDisturbanceBuffer, int? coordinateUncertaintyInMeters)
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

            location.DecimalLongitude = Math.Round(point.X, 5);
            location.DecimalLatitude = Math.Round(point.Y, 5);
            location.Point = point.ToGeoShape() as PointGeoShape;
            location.PointLocation = point.ToGeoLocation();

            location.PointWithBuffer = pointWithBuffer;
            location.PointWithDisturbanceBuffer = pointWithDisturbanceBuffer;

            var coordinateConversionTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.CoordinateConversion);
            var sweRef99TmPoint = point.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
            location.Sweref99TmX = Math.Round(sweRef99TmPoint.Coordinate.X, 0);
            location.Sweref99TmY = Math.Round(sweRef99TmPoint.Coordinate.Y, 0);
           
            var etrs89Point = point.Transform(CoordinateSys.WGS84, CoordinateSys.ETRS89_LAEA_Europe);
            location.Etrs89X = Math.Round(etrs89Point.Coordinate.X, 7);
            location.Etrs89Y = Math.Round(etrs89Point.Coordinate.Y, 7);
            TimeManager.Stop(ProcessTimeManager.TimerTypes.CoordinateConversion, coordinateConversionTimerSessionId);

            location.VerbatimSRS = verbatimCoordinateSystem.EpsgCode();

            if (!verbatimLatitude.HasValue || !verbatimLongitude.HasValue)
            {
                return;
            }
            location.VerbatimLatitude = verbatimLatitude.Value.ToString(CultureInfo.InvariantCulture);
            location.VerbatimLongitude = verbatimLongitude.Value.ToString(CultureInfo.InvariantCulture);
        }        

        protected FactoryBase(DataProvider dataProvider, IProcessTimeManager processTimeManager, ProcessConfiguration processConfiguration)
        {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            TimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            ProcessConfiguration = processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));
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
            const int maxCoordinateUncertaintyInMeters = 500000; // 500 km
            Point point = null!;            
            if (verbatimLongitude.HasValue && verbatimLongitude.Value > 0 && verbatimLatitude.HasValue && verbatimLatitude > 0)
            {
                point = new Point(verbatimLongitude.Value, verbatimLatitude.Value);

                if (verbatimCoordinateSystem != CoordinateSys.WGS84)
                {
                    point = point.Transform(verbatimCoordinateSystem, CoordinateSys.WGS84) as Point;
                }
            }

            if (!coordinateUncertaintyInMeters.HasValue || coordinateUncertaintyInMeters.Value <= 0)
            {
                coordinateUncertaintyInMeters = DataProvider.CoordinateUncertaintyInMeters;
            }
            if (coordinateUncertaintyInMeters.Value > maxCoordinateUncertaintyInMeters)
            {
                coordinateUncertaintyInMeters = maxCoordinateUncertaintyInMeters;
            }

            var pointWithBuffer = point.ToCircle(coordinateUncertaintyInMeters!.Value);
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
            GeoJsonGeometry<GeoJson2DCoordinates> pointWithBuffer, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
        {
            if ((coordinateUncertaintyInMeters ?? 0) == 0)
            {
                coordinateUncertaintyInMeters = DataProvider.CoordinateUncertaintyInMeters;
            }

            var pointWithDisturbanceBuffer = GetPointWithDisturbanceBuffer(point, taxonDisturbanceRadius);

            InitializeLocation(location, verbatimLongitude, verbatimLatitude, verbatimCoordinateSystem, point, pointWithBuffer?.ToGeoShape() as PolygonGeoShape, pointWithDisturbanceBuffer?.ToGeoShape() as PolygonGeoShape, coordinateUncertaintyInMeters);
        }

        /// <summary>
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="accessRightsId"></param>
        /// <returns></returns>
        protected int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon? taxon)
        {
            return CalculateProtectionLevel(taxon, null);
        }

        /// <summary>
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="accessRightsId"></param>
        /// <returns></returns>
        protected int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon? taxon, AccessRightsId? accessRightsId)
        {
            if (accessRightsId is AccessRightsId.FreeUsage) return 1;
            var protectionLevel = taxon?.Attributes?.SensitivityCategory?.Id ?? 1;
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