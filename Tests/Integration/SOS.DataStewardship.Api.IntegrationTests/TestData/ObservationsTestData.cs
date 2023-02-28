using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.TestData
{
    internal class ObservationsTestData
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
                    .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3, new[] { firstDatasetId }))
                    .With(m => m.DataProviderId = 1)
                    .With(m => m.ArtportalenInternal = null)
                    .With(m => m.Sensitive = false)
                .Build();

            return observations;
        }

    }
}
