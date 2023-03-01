using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.DataStewardship.Api.IntegrationTests.TestData
{
    internal class EventsTestData
    {
        public static IEnumerable<ObservationEvent> GetEventTestData(string firstEventKey = null, string? firstDatasetKey = null)
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

            return events;
        }
    }
}
