using FizzWare.NBuilder.Implementation;
using SOS.DataStewardship.Api.IntegrationTests.Core.Helpers;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    internal static class ObservationsTestData
    {
        public static IEnumerable<Observation> GetObservationTestData(string firstEventKey = null, string firstDatasetId = null)
        {
            firstEventKey ??= DataHelper.RandomString(3);
            firstDatasetId ??= DataHelper.RandomString(3);

            var observations = Builder<Observation>.CreateListOfSize(10)
                 .TheFirst(1)
                    .With(m => m.Event = new Event
                    {
                        EventId = firstEventKey,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                    })
                    .With(m => m.Occurrence = new Occurrence
                    {
                        OccurrenceId = DataHelper.RandomString(3)
                    })                    
                    .With(m => m.DataStewardshipDatasetId = firstDatasetId)
                    .With(m => m.DataProviderId = 1)
                    .With(m => m.ArtportalenInternal = null)
                    .With(m => m.Sensitive = false)
                .TheNext(9)
                    .With(m => m.Event = new Event
                    {
                        EventId = DataHelper.RandomString(3, new[] { firstEventKey }),
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                    })
                    .With(m => m.Occurrence = new Occurrence
                    {
                        OccurrenceId = DataHelper.RandomString(3)
                    })
                    .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3, new[] { firstDatasetId }))
                    .With(m => m.DataProviderId = 1)
                    .With(m => m.ArtportalenInternal = null)
                    .With(m => m.Sensitive = false)
                .Build();

            return observations;
        }


        public static IOperable<Observation> GetObservationTestDataBuilder(int size)
        {
            var observationsBuilder = Builder<Observation>.CreateListOfSize(size)
                .All()
                    .With(m => m.Event = new Event
                    {
                        EventId = DataHelper.RandomString(8),
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                    })
                    .With(m => m.Occurrence = new Occurrence
                    {
                        OccurrenceId = DataHelper.RandomString(8)
                    })
                    .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(8))
                    .With(m => m.DataProviderId = 1)
                    .With(m => m.ArtportalenInternal = null)
                    .With(m => m.Sensitive = false);

            return observationsBuilder;
        }

        public static IOperable<Observation> HaveDatasetIds(this IOperable<Observation> operable, IEnumerable<string> datasetIds)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            var datasetsIdsList = datasetIds.ToList();
            builder.With((obs, index) =>
            {
                var datasetIdsIndex = index % datasetsIdsList.Count;
                var datasetId = datasetsIdsList[datasetIdsIndex];
                obs.DataStewardshipDatasetId = datasetId;
            });

            return operable;
        }

        public static IOperable<Observation> HaveEventIds(this IOperable<Observation> operable, IEnumerable<string> eventIds)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            var eventIdsList = eventIds.ToList();
            builder.With((obs, index) =>
            {
                int eventIdsIndex = index % eventIdsList.Count;
                string eventId = eventIdsList[eventIdsIndex];
                obs.Event.EventId = eventId;
            });

            return operable;
        }

        public static IOperable<Observation> WithDate(this IOperable<Observation> operable, DateTime date)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;            
            builder.With((obs, index) =>
            {                
                obs.Event.StartDate = date;
                obs.Event.EndDate = date;
            });

            return operable;
        }

        public static IOperable<Observation> WithDates(this IOperable<Observation> operable, DateTime startDate, DateTime endDate)
        {
            var builder = ((IDeclaration<Observation>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Event.StartDate = startDate;
                obs.Event.EndDate = endDate;
            });

            return operable;
        }
    }
}