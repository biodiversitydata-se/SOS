using System;
using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Processors.Artportalen;
using SOS.Process.UnitTests.TestHelpers.Factories;
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
            var factory = ArtportalenObservationFactory.CreateAsync(
                dataProviderDummy,
                new Dictionary<int, Taxon>(), 
                vocabularyRepository.Object,
                false).Result;
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