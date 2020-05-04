using System;
using System.Diagnostics;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;

namespace SOS.Process.UnitTests.TestHelpers
{
    /// <summary>
    /// A fixture that creates an instance of DwcaObservationFactory with the following properties:
    /// - Only Mammalia taxa are used.
    /// - Only County and Province regions are used.
    /// - All field mappings are used.
    ///
    /// All data is loaded from files in the Resources folder.
    /// </summary>
    public class DwcaObservationFactoryFixture : IDisposable
    {
        public DwcaObservationFactory DwcaObservationFactory { get; private set; }

        public DwcaObservationFactoryFixture()
        {
            DwcaObservationFactory = CreateDwcaObservationFactory();
        }

        public void Dispose()
        {
            DwcaObservationFactory = null;
        }

        private DwcaObservationFactory CreateDwcaObservationFactory()
        {
            var mammaliaTaxa = MessagePackHelper.CreateListFromMessagePackFile<ProcessedTaxon>(@"Resources\MammaliaProcessedTaxa.msgpck");
            var mammaliaTaxonByTaxonId = mammaliaTaxa.ToDictionary(t => t.Id, t => t);
            var processedAreaRepositoryStub = ProcessedAreaRepositoryStubFactory.Create(AreaType.County, AreaType.Province);
            var processedFieldMappingRepository = ProcessedFieldMappingRepositoryStubFactory.Create();
            var areaHelper = new AreaHelper(processedAreaRepositoryStub.Object, processedFieldMappingRepository.Object);
            var factory = DwcaObservationFactory.CreateAsync(
                mammaliaTaxonByTaxonId, 
                processedFieldMappingRepository.Object,
                areaHelper).Result;
            return factory;
        }
    }
}