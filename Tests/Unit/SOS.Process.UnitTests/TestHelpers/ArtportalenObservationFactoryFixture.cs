using System;
using System.Linq;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;
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
            var mammaliaTaxa =
                MessagePackHelper.CreateListFromMessagePackFile<Taxon>(
                    @"Resources\MammaliaProcessedTaxa.msgpck");
            var mammaliaTaxonByTaxonId = mammaliaTaxa.ToDictionary(t => t.Id, t => t);
            var vocabularyRepository = VocabularyRepositoryStubFactory.Create();
            var areaHelperStub = new Mock<IAreaHelper>();            
            var factory = ArtportalenObservationFactory.CreateAsync(
                dataProviderDummy,
                mammaliaTaxonByTaxonId,
                vocabularyRepository.Object,
                areaHelperStub.Object,
                false, false).Result;
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