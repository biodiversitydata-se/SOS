﻿using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Observations.Api.IntegrationTests.Helpers;

namespace SOS.Observations.Api.IntegrationTests.TestData.Factories;
internal class DatasetBuilderFactory
{
    public static IOperable<Dataset> Create(int size = 10)
    {
        var datasetBuilder = Builder<Dataset>.CreateListOfSize(size)
            .All()
                .With(m => m.Identifier = DataHelper.RandomString(8))
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null);
        return datasetBuilder;
    }
}

