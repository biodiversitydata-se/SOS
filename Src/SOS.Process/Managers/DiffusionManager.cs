using System;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Managers
{
    public class DiffusionManager : IDiffusionManager
    {
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Diffuse the point based on the sightings protection level
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="protectionLevel"></param>
        private void DiffuseGeographicalData(Observation observation)
        {
            if (observation.Location == null)
            {
                return;
            }

            var location = observation.Location;

            // Save coordinates
            var latitude = location.DecimalLatitude.HasValue ? location.DecimalLatitude.Value : 0;
            var longitude = location.DecimalLongitude.HasValue ? location.DecimalLongitude.Value : 0;

            if (observation?.ArtportalenInternal != null)
            {
                observation.ArtportalenInternal.BirdValidationAreaIds = null;
                observation.ArtportalenInternal.LocationExternalId = null;
                observation.ArtportalenInternal.LocationPresentationNameParishRegion = null;
                observation.ArtportalenInternal.ParentLocality = null;
                observation.ArtportalenInternal.ParentLocationId = null;
            }

            // Make sure all location data are erased
            location.DecimalLatitude = null;
            location.DecimalLongitude = null;
            location.County = null;
            location.Locality = null;
            location.Municipality = null;
            location.Parish = null;
            location.Point = null;
            location.PointLocation = null;
            location.PointWithBuffer = null;
            location.Province = null;
            location.VerbatimCoordinates = null;
            location.VerbatimLatitude = null;
            location.VerbatimLongitude = null;
            location.VerbatimLocality = null;
            location.Attributes.VerbatimMunicipality = null;
            location.Attributes.VerbatimProvince = null;

            if (latitude == 0 || longitude == 0)
            {
                return;
            }

            var (mod, add) = GetDiffusionValues(observation.Occurrence?.ProtectionLevel ?? 0);

           
            //transform the point into the same format as Artportalen so that we can use the same diffusion as them
            var geompoint = new NetTopologySuite.Geometries.Point(longitude, latitude);
            var transformedPoint = geompoint.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);
            var diffusedUntransformedPoint = new NetTopologySuite.Geometries.Point(
                transformedPoint.Coordinates[0].X - transformedPoint.Coordinates[0].X % mod +
                add,
                transformedPoint.Coordinates[0].Y - transformedPoint.Coordinates[0].Y % mod +
                add);

            //retransform to the correct format again
            var coordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters ?? 0;
            var newCoordinateUncertaintyInMeters = coordinateUncertaintyInMeters > mod ? coordinateUncertaintyInMeters : mod;
            var diffusedPoint =
                diffusedUntransformedPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
            var diffusedPolygon = ((NetTopologySuite.Geometries.Point) diffusedPoint).ToCircle(newCoordinateUncertaintyInMeters);

            // Set location diffused geographical data
            location.DecimalLongitude = diffusedPoint.Coordinate.X;
            location.DecimalLatitude = diffusedPoint.Coordinate.Y;
            location.CoordinateUncertaintyInMeters = newCoordinateUncertaintyInMeters;
            location.Point = (PointGeoShape) diffusedPoint.ToGeoShape();
            location.PointLocation = location.Point.ToGeoLocation();
            location.PointWithBuffer = (PolygonGeoShape) diffusedPolygon.ToGeoShape();

            _areaHelper.AddAreaDataToProcessedObservation(observation);
        }

        /// <summary>
        /// Get level of diffusion based on protection level
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <returns></returns>
        private (int mod, int add) GetDiffusionValues(int protectionLevel)
        {
            return protectionLevel switch
            {
                2 => (1000, 555),
                3 => (5000, 2505),
                4 => (25000, 12505),
                5 => (50000, 25005),
                _ => (1, 0)
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public DiffusionManager(IAreaHelper areaHelper, ILogger<DiffusionManager> logger)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public void DiffuseObservation(Observation observation)
        {
            if (observation == null)
            {
                return;
            }

            observation.DiffuseStatus = DiffuseStatus.DiffusedBySystem;
            observation.DataGeneralizations += " All data related to the exact location of the observation has been diffused or removed. Native data is available with extended privileges.";

            // Diffused observations is not protected
            observation.Protected = false;
            observation.Occurrence.ReportedBy = string.Empty;
            
            if (observation.Modified.HasValue)
            {
                observation.Modified = new DateTime(observation.Modified.Value.Year, observation.Modified.Value.Month, 1);
            }

            if (observation.Occurrence.ReportedDate.HasValue)
            {
                observation.Occurrence.ReportedDate = new DateTime(observation.Occurrence.ReportedDate.Value.Year, observation.Occurrence.ReportedDate.Value.Month, 1);
            }

            if (observation.ArtportalenInternal != null)
            {
                observation.ArtportalenInternal.ReportedByUserAlias = string.Empty;
                observation.ArtportalenInternal.ReportedByUserId = null;
                observation.ArtportalenInternal.OccurrenceRecordedByInternal = null;
            }

            if (observation.Event != null)
            {
                if (observation.Event.StartDate.HasValue)
                {
                    observation.Event.StartDate = new DateTime(observation.Event.StartDate.Value.Year, observation.Event.StartDate.Value.Month, 1);
                }

                if (observation.Event.EndDate.HasValue)
                {
                    observation.Event.EndDate = new DateTime(observation.Event.EndDate.Value.Year, observation.Event.EndDate.Value.Month, 1);
                }

                observation.Event.VerbatimEventDate = DwcFormatter.CreateDateIntervalString(observation.Event.StartDate, observation.Event.EndDate);
            }

            if (observation.Occurrence != null)
            {
                observation.Occurrence.OccurrenceRemarks = string.Empty;
                observation.Occurrence.RecordedBy = string.Empty;
            }

            DiffuseGeographicalData(observation);
        }
    }
}
