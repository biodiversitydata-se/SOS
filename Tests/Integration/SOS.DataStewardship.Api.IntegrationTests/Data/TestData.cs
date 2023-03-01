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
    }
}
