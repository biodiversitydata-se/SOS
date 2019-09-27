using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using ProcessService.Application.GIS;
using SOS.Core.Enums;
using SOS.Core.GIS;
using SOS.Core.Models.Observations;

namespace SOS.Core.ObservationProcessors
{
    public class TestDataProviderProcessor
    {
        public ProcessedDwcObservation ProcessObservation(VerbatimTestDataProviderObservation observation)
        {
            ProcessedDwcObservation processedObservation = new ProcessedDwcObservation();

            // Copied values
            processedObservation.RecordedBy = observation.ObserverName;
            processedObservation.CoordinateX = observation.XCoord;
            processedObservation.CoordinateY = observation.YCoord;
            processedObservation.DecimalLatitude = observation.YCoord;
            processedObservation.DecimalLongitude = observation.XCoord;
            processedObservation.ObservationDateStart = observation.ObservedDate;
            processedObservation.CatalogNumber = observation.Id.ToString();
            processedObservation.OccurrenceID = $"urn:lsid:TestDataProvider.se:Sighting:{observation.Id}";

            // Calculated values
            processedObservation.Modified = DateTime.UtcNow;
            processedObservation.DyntaxaTaxonId = ParseDyntaxaTaxonIdFromScientificName(observation.ScientificName);
            CalculateCoordinateValues(observation, processedObservation);
            FileBasedGeographyService.CalculateRegionBelongings(processedObservation);

            // Static values for this data provider
            processedObservation.CoordinateUncertaintyInMeters = 500;
            processedObservation.DataProviderId = 25;
            processedObservation.DatasetName = "TestDataset";
            processedObservation.DatasetID = "urn:lsid:swedishlifewatch.se:DataProvider:25";
            processedObservation.IsPublic = true;
            processedObservation.Type = "Occurrence";
            processedObservation.BasisOfRecord = "HumanObservation";
            processedObservation.UncertainDetermination = false;
            processedObservation.IsNaturalOccurrence = true;
            processedObservation.IsPositiveObservation = true;
            processedObservation.IsNeverFoundObservation = false;
            processedObservation.IsNotRediscoveredObservation = false;
            processedObservation.Rights = "Free usage"; // todo - merge with AccessRights?
            processedObservation.AccessRights = "Free usage"; // todo - merge with Rights
            processedObservation.RightsHolder = "Test data provider";
            processedObservation.Language = "Swedish";

            return processedObservation;
        }

        private int ParseDyntaxaTaxonIdFromScientificName(string scientificName)
        {
            switch (scientificName)
            {
                case "Cyanistes caeruleus": // Blåmes [103025]
                    return 103025;
                case "Psophus stridulus": // Trumgräshoppa [101656]
                    return 101656;
                case "Haliaeetus albicilla": // Havsörn [100067]
                    return 100067;
                case "Canis lupus": // Varg [100024]
                    return 100024;
                case "Tussilago farfara": // Tussilago (hästhov) [220396]
                    return 220396;
                default:
                    throw new ArgumentException($"Can't parse scientific name: \"{scientificName}\"");
            }
        }

        protected virtual void CalculateCoordinateValues(
            VerbatimTestDataProviderObservation verbatimObservation,
            ProcessedDwcObservation observation)
        {
            IPoint wgs84Point = new Point(verbatimObservation.XCoord, verbatimObservation.YCoord);

            IPoint webMercatorPoint = CoordinateConversionManager.GetConvertedPoint(
                wgs84Point,
                CoordinateSystemId.WGS84,
                CoordinateSystemId.WebMercator);
            IPoint sweref99TmPoint = CoordinateConversionManager.GetConvertedPoint(
                wgs84Point,
                CoordinateSystemId.WGS84,
                CoordinateSystemId.SWEREF99_TM);
            IPoint rt90Point = CoordinateConversionManager.GetConvertedPoint(
                wgs84Point,
                CoordinateSystemId.WGS84,
                CoordinateSystemId.Rt90_25_gon_v);

            observation.CoordinateX = wgs84Point.X;
            observation.CoordinateY = wgs84Point.Y;
            observation.GeodeticDatum = "WGS84";
            //obs.Coordinate_RT90 = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(rt90Point.X, rt90Point.Y) // todo - Add point class to FlatDarwinCoreObservation
            observation.CoordinateX_RT90 = rt90Point.X;
            observation.CoordinateY_RT90 = rt90Point.Y;

            //obs.Coordinate_SWEREF99TM = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(sweref99TmPoint.X, sweref99TmPoint.Y) // todo - Add point class to FlatDarwinCoreObservation
            observation.CoordinateX_SWEREF99TM = sweref99TmPoint.X;
            observation.CoordinateY_SWEREF99TM = sweref99TmPoint.Y;

            //obs.Coordinate_WebMercator = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(googleMercatorPoint.X, googleMercatorPoint.Y) // todo - Add point class to FlatDarwinCoreObservation
            observation.CoordinateX_WebMercator = webMercatorPoint.X;
            observation.CoordinateY_WebMercator = webMercatorPoint.Y;
        }
    }
}
