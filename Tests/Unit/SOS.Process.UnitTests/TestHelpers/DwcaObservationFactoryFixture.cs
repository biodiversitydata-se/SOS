using System;
using System.Linq;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Managers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;
using Xunit;

namespace SOS.Process.UnitTests.TestHelpers
{
    /// <summary>
    ///     A fixture that creates an instance of DwcaObservationFactory with the following properties:
    ///     - Only Mammalia taxa are used.
    ///     - Only County and Province regions are used.
    ///     - All vocabulary mappings are used.
    ///     All data is loaded from files in the Resources folder.
    /// </summary>
    public class DwcaObservationFactoryFixture : IDisposable
    {
        public DwcaObservationFactoryFixture()
        {
            DwcaObservationFactory = CreateDwcaObservationFactory();
        }

        public DwcaObservationFactory DwcaObservationFactory { get; private set; }

        public void Dispose()
        {
            DwcaObservationFactory = null;
        }

        private DwcaObservationFactory CreateDwcaObservationFactory()
        {
            var dataProviderDummy = new DataProvider();
            var mammaliaTaxa =
                MessagePackHelper.CreateListFromMessagePackFile<Taxon>(
                    @"Resources\MammaliaProcessedTaxa.msgpck");
            var mammaliaTaxonByTaxonId = mammaliaTaxa.ToDictionary(t => t.Id, t => t);
            var processedAreaRepositoryStub =
                ProcessedAreaRepositoryStubFactory.Create(AreaType.County, AreaType.Province);
            var vocabularyRepository = VocabularyRepositoryStubFactory.Create();
            var areaHelper = new AreaHelper(processedAreaRepositoryStub.Object);
            var factory = DwcaObservationFactory.CreateAsync(
                dataProviderDummy,
                mammaliaTaxonByTaxonId,
                vocabularyRepository.Object,
                areaHelper,
                new ProcessTimeManager(new ProcessConfiguration())).Result;
            return factory;
        }
    }

    [CollectionDefinition("DwcaObservationFactory collection")]
    public class DwcaObservationFactoryCollection : ICollectionFixture<DwcaObservationFactoryFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}