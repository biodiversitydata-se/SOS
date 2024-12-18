﻿using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.Artportalen;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Process.UnitTests.TestHelpers.Factories;
using System;
using System.Collections.Generic;
using Xunit;

namespace SOS.Process.UnitTests.TestHelpers
{
    /// <summary>
    ///     A fixture that creates an instance of ArtportalenObservationFactory with the following properties:
    ///     - Only Mammalia taxa are used.
    ///     - All vocabularies are used.
    ///     All data is loaded from files in the Resources folder.
    /// </summary>
    public class ArtportalenObservationFactoryFixture : IDisposable
    {
        public ArtportalenObservationFactoryFixture()
        {
            ArtportalenObservationFactory = CreateArtportalenObservationFactory();
        }

        public ArtportalenObservationFactory ArtportalenObservationFactory { get; private set; }

        public void Dispose()
        {
            ArtportalenObservationFactory = null;
        }

        private ArtportalenObservationFactory CreateArtportalenObservationFactory()
        {
            var dataProviderDummy = new DataProvider();
            var vocabularyRepository = VocabularyRepositoryStubFactory.Create();
            var datasetRepositoryMock = new Mock<IArtportalenDatasetMetadataRepository>();
            var processConfiguration = new ProcessConfiguration();
            var factory = ArtportalenObservationFactory.CreateAsync(
                dataProviderDummy,
                new Dictionary<int, Taxon>(),
                vocabularyRepository.Object,
                datasetRepositoryMock.Object,
                false,
                "https://artportalen-st.artdata.slu.se",
                new ProcessTimeManager(processConfiguration),
                processConfiguration).Result;
            return factory;
        }
    }

    [CollectionDefinition("ArtportalenObservationFactory collection")]
    public class ArtportalenObservationFactoryCollection : ICollectionFixture<ArtportalenObservationFactoryFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}