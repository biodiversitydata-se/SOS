using FizzWare.NBuilder.Implementation;
using SOS.Lib.Models.Processed.DataStewardship.Event;

namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    internal static class EventsBuilderFactory
    {        
        public static IOperable<ObservationEvent> Create(int size)            
        {            
            var eventsBuilder = Builder<ObservationEvent>.CreateListOfSize(size)
                 .All()
                    .With(m => m.EventId = DataHelper.RandomString(8))
                    .With(m => m.StartDate = DateTime.Now)
                    .With(m => m.EndDate = DateTime.Now)                        
                    .With(m => m.Dataset = new DatasetInfo
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