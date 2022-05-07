﻿using System.Globalization;
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

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ObservationFactoryBase
    {
        protected readonly DataProvider DataProvider;
        protected IDictionary<int, Lib.Models.Processed.Observation.Taxon> Taxa { get; }
        protected readonly IProcessTimeManager TimeManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationFactoryBase(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IProcessTimeManager processTimeManager)
        {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            TimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
        }
        
        /// <summary>
        /// Get taxon
        /// </summary>
        /// <param name="taxonId"></param>
        /// <returns></returns>
        protected Lib.Models.Processed.Observation.Taxon GetTaxon(int taxonId)
        {
            Taxa.TryGetValue(taxonId, out var taxon);

            return taxon ?? new Lib.Models.Processed.Observation.Taxon { Id = -1, VerbatimId = taxonId.ToString() };
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
          
            var sweref99TimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.Sweref99Conversion);
            var sweRef99TmPoint = point.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
            location.Sweref99TmX = sweRef99TmPoint.Coordinate.X;
            location.Sweref99TmY = sweRef99TmPoint.Coordinate.Y;
            TimeManager.Stop(ProcessTimeManager.TimerTypes.Sweref99Conversion, sweref99TimerSessionId);

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
            if ((taxonDisturbanceRadius ?? 0) <= 0)
            {
                return null!;
            }

            return point.ToCircle(taxonDisturbanceRadius!.Value);
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
            GeoJsonGeometry pointWithBuffer, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
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

        protected bool IsSensitiveObservation(Observation observation)
        {
            return observation.Occurrence.SensitivityCategory > 2 || observation.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage;            
        }

        /// <summary>
        /// Populate some generic data
        /// </summary>
        /// <param name="observation"></param>
        protected void PopulateGenericData(Observation observation)
        {
            if (observation.Event?.StartDate != null)
            {
                var startDate = observation.Event.StartDate.Value.ToLocalTime();
                observation.Event.StartYear = startDate.Year;
                observation.Event.StartMonth = startDate.Month;
                observation.Event.StartDay = startDate.Day;
            }

            if (observation.Event?.EndDate != null)
            {
                var endDate = observation.Event.EndDate.Value.ToLocalTime();
                observation.Event.EndYear = endDate.Year;
                observation.Event.EndMonth = endDate.Month;
                observation.Event.EndDay = endDate.Day;
            }

            if (observation.Event?.StartDate == null ||
                (observation.Taxon?.Id ?? 0) == 0 ||
                (observation.Location?.DecimalLatitude ?? 0) == 0 ||
                (observation.Location?.DecimalLongitude ?? 0) == 0)
            {
                return;
            }
            // Round coordinates to 5 decimals (roughly 1m)
            var source = $"{observation.Event.StartDate.Value.ToUniversalTime().ToString("s")}-{observation.Taxon.Id}-{Math.Round(observation.Location.DecimalLongitude.Value, 5)}/{Math.Round(observation.Location.DecimalLatitude.Value, 5)}";

            observation.DataQuality = new DataQuality
            {
                UniqueKey = source.ToHash()
            };
        }
    }
}
