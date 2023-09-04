﻿using FizzWare.NBuilder.Implementation;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Observations.Api.IntegrationTests.Helpers;

namespace SOS.Observations.Api.IntegrationTests.TestData.Factories;
internal static class EventsBuilderFactory
{
    public static IOperable<Event> Create(int size)
    {
        var eventsBuilder = Builder<Event>.CreateListOfSize(size)
             .All()
                .With(m => m.EventId = DataHelper.RandomString(8))
                .With(m => m.StartDate = DateTime.Now)
                .With(m => m.EndDate = DateTime.Now)
                .With(m => m.PlainStartDate = DateTime.Now.ToString("yyyy-MM-dd"))
                .With(m => m.PlainEndDate = DateTime.Now.ToString("yyyy-MM-dd"))
                .With(m => m.PlainStartTime = DateTime.Now.ToString("HH:mm:ss"))
                .With(m => m.PlainEndTime = DateTime.Now.ToString("HH:mm:ss"))
                .With(m => m.DataStewardship = new DataStewardshipInfo
                {
                    DatasetIdentifier = DataHelper.RandomString(8)
                });

        return eventsBuilder;
    }

    public static IOperable<Event> HaveDatasetIds(this IOperable<Event> operable, IEnumerable<string> datasetIds)
    {
        var builder = ((IDeclaration<Event>)operable).ObjectBuilder;
        var datasetsIdsList = datasetIds.ToList();
        builder.With((ev, index) =>
        {
            var datasetIdsIndex = index % datasetsIdsList.Count;
            var datasetId = datasetsIdsList[datasetIdsIndex];
            ev.DataStewardship.DatasetIdentifier = datasetId;
        });

        return operable;
    }
}