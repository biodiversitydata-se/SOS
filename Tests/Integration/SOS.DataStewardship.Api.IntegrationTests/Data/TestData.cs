﻿using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    public static class TestData
    {
        public static TestDataSet Create(int size = 10)
        {
            var datasetsBuilder = DatasetBuilderFactory.Create(size);
            var datasets = datasetsBuilder.Build().ToList();
            
            var eventsBuilder = EventsBuilderFactory.Create(size);
            eventsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
            var events = eventsBuilder.Build().ToList();

            IOperable<Observation> observationsBuilder = ObservationsBuilderFactory.Create(size);
            observationsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
            observationsBuilder.All().HaveEventIds(events.Select(m => m.EventId));
            var observations = observationsBuilder.Build().ToList();

            return new TestDataSet
            {
                Datasets = datasets,
                Events = events,
                Observations = observations,
                DatasetsBuilder = datasetsBuilder,
                EventsBuilder = eventsBuilder,
                ObservationsBuilder = observationsBuilder
            };
        }
        
        public static GeometryFilter CreatePolygonFilterFromBbox(double minLon, double minLat, double maxLon, double maxLat)
        {
            var filter = new GeometryFilter
            {

                GeographicArea = new Polygon(
                    new LinearRing([
                        new Coordinate(maxLon, minLat),
                        new Coordinate(maxLon, maxLat),
                        new Coordinate(minLon, maxLat),
                        new Coordinate(minLon, minLat),
                        new Coordinate(maxLon, minLat)
                    ])
                )
            };

            return filter;
        }

        public static GeometryFilter CreateCircleFilterFromPoint(double lat, double lon, double distance)
        {
            var filter = new GeometryFilter 
            {
                GeographicArea = new Point(lon, lat),
                MaxDistanceFromGeometries = distance
            };

            return filter;
        }

        public class TestDataSet
        {
            public List<Lib.Models.Processed.DataStewardship.Dataset.Dataset> Datasets { get; set; }
            public List<Lib.Models.Processed.DataStewardship.Event.Event> Events { get; set; }
            public List<Observation> Observations { get; set; }
            public IOperable<Lib.Models.Processed.DataStewardship.Dataset.Dataset> DatasetsBuilder { get; set; }
            public IOperable<Lib.Models.Processed.DataStewardship.Event.Event> EventsBuilder { get; set; }
            public IOperable<Observation> ObservationsBuilder { get; set; }
        }
    }
}
