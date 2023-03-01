using FizzWare.NBuilder.Implementation;
using SOS.Lib.Models.Processed.DataStewardship.Event;

namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    internal static class EventsTestData
    {
        public static List<ObservationEvent> GetEventTestData(string firstEventKey = null, string? firstDatasetKey = null)
        {
            firstEventKey ??= DataHelper.RandomString(3);
            firstDatasetKey ??= DataHelper.RandomString(3);

            var events = Builder<ObservationEvent>.CreateListOfSize(10)
                 .TheFirst(1)
                    .With(m => m.EventId = firstEventKey)
                    .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                    {
                        Identifier = firstDatasetKey,
                    })
                .TheNext(9)
                     .With(m => m.EventId = DataHelper.RandomString(3, new[] { firstEventKey }))
                     .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                     {
                         Identifier = DataHelper.RandomString(3, new[] { firstDatasetKey }),
                     })
                .Build();

            return events.ToList();
        }

        public static IOperable<ObservationEvent> GetEventTestDataBuilder(int size)            
        {            
            var eventsBuilder = Builder<ObservationEvent>.CreateListOfSize(10)
                 .All()
                    .With(m => m.EventId = DataHelper.RandomString(8))
                    .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                    {
                        Identifier = DataHelper.RandomString(8)
                    });

            return eventsBuilder;
        }

        public static IOperable<ObservationEvent> HaveDatasetIds(this IOperable<ObservationEvent> operable, IEnumerable<string> datasetIds)
        {
            var builder = ((IDeclaration<ObservationEvent>)operable).ObjectBuilder;
            var datasetsIdsList = datasetIds.ToList();
            builder.With((ev, index) =>
            {
                var datasetIdsIndex = index % datasetsIdsList.Count;
                var datasetId = datasetsIdsList[datasetIdsIndex];
                ev.Dataset.Identifier = datasetId;
            });

            return operable;
        }
    }
}