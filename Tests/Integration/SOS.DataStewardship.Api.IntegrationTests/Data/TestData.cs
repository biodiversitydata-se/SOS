using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    internal static class TestData
    {
        public static (List<ObservationDataset> datasets, List<ObservationEvent> events, List<Observation> observations) GetTestData()
        {
            const int size = 10;
            var datasetBuilder = DatasetBuilder.GetDatasetTestDataBuilder(size);
            var datasets = datasetBuilder.Build().ToList();

            var eventsBuilder = EventsTestData.GetEventTestDataBuilder(size);
            eventsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
            var events = eventsBuilder.Build().ToList();

            var observationsBuilder = ObservationsTestData.GetObservationTestDataBuilder(size);
            observationsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
            observationsBuilder.All().HaveEventIds(events.Select(m => m.EventId));
            var observations = observationsBuilder.Build().ToList();

            return (datasets, events, observations);
        }

        //public static (List<ObservationDataset> datasets, List<ObservationEvent> events, IOperable<Observation> observationsBuilder) GetTestDataObservationsBuilder(int size = 10)
        //{
        //    var datasetBuilder = DatasetBuilder.GetDatasetTestDataBuilder(size);
        //    var datasets = datasetBuilder.Build().ToList();

        //    var eventsBuilder = EventsTestData.GetEventTestDataBuilder(size);
        //    eventsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
        //    var events = eventsBuilder.Build().ToList();

        //    IOperable<Observation> observationsBuilder = ObservationsTestData.GetObservationTestDataBuilder(size);
        //    observationsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
        //    observationsBuilder.All().HaveEventIds(events.Select(m => m.EventId));            

        //    return (datasets, events, observationsBuilder);
        //}

        public static TestDataSet GetTestDataSet(int size = 10)
        {
            var datasetsBuilder = DatasetBuilder.GetDatasetTestDataBuilder(size);
            var datasets = datasetsBuilder.Build().ToList();

            var eventsBuilder = EventsTestData.GetEventTestDataBuilder(size);
            eventsBuilder.All().HaveDatasetIds(datasets.Select(m => m.Identifier));
            var events = eventsBuilder.Build().ToList();

            IOperable<Observation> observationsBuilder = ObservationsTestData.GetObservationTestDataBuilder(size);
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


        internal class TestDataSet
        {
            public List<ObservationDataset> Datasets { get; set; }
            public List<ObservationEvent> Events { get; set; }
            public List<Observation> Observations { get; set; }
            public IOperable<ObservationDataset> DatasetsBuilder { get; set; }
            public IOperable<ObservationEvent> EventsBuilder { get; set; }
            public IOperable<Observation> ObservationsBuilder { get; set; }
        }
    }
}
