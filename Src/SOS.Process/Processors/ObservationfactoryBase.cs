 using System;
using System.Globalization;
using System.Threading.Tasks;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Process.Processors
{
    public class ObservationfactoryBase
    {
        private readonly IGeometryManager _geometryManager;
        
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
        private void InitializeLocation(Location location, double? verbatimLongitude, double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, Point point, Geometry pointWithBuffer, Geometry pointWithDisturbanceBuffer, int? coordinateUncertaintyInMeters)
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
            location.Point = (PointGeoShape)point.ToGeoShape();
            location.PointLocation = point.ToGeoLocation();
            location.PointWithBuffer = (PolygonGeoShape)pointWithBuffer?.ToGeoShape();
            location.PointWithDisturbanceBuffer = (PolygonGeoShape)pointWithDisturbanceBuffer?.ToGeoShape();

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
        private async Task<Geometry> GetPointWithDisturbanceBufferAsync(Point point, int? taxonDisturbanceRadius)
        {
            if (!(taxonDisturbanceRadius.HasValue && taxonDisturbanceRadius.Value > 0))
            {
                return null;
            }

            return await _geometryManager.GetCircleAsync(point, taxonDisturbanceRadius);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geometryManager"></param>
        protected ObservationfactoryBase(IGeometryManager geometryManager)
        {
            _geometryManager = geometryManager ?? throw new ArgumentNullException(nameof(geometryManager));
        }

        protected async Task AddPositionData(Location location, double? verbatimLongitude, double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
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

            var pointWithBuffer = await _geometryManager.GetCircleAsync(point, coordinateUncertaintyInMeters);
            var pointWithDisturbanceBuffer = await GetPointWithDisturbanceBufferAsync(point, taxonDisturbanceRadius);

            InitializeLocation(location, verbatimLongitude, verbatimLatitude, verbatimCoordinateSystem, point, pointWithBuffer, pointWithDisturbanceBuffer, coordinateUncertaintyInMeters);
        }

        protected async Task AddPositionData(Location location, double? verbatimLongitude,
            double? verbatimLatitude, CoordinateSys verbatimCoordinateSystem, Point point,
            Geometry pointWithBuffer, int? coordinateUncertaintyInMeters, int? taxonDisturbanceRadius)
        {
            var pointWithDisturbanceBuffer = await GetPointWithDisturbanceBufferAsync(point, taxonDisturbanceRadius);

            InitializeLocation(location, verbatimLongitude, verbatimLatitude, verbatimCoordinateSystem, point, pointWithBuffer, pointWithDisturbanceBuffer, coordinateUncertaintyInMeters);
        }
    }
}
