using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Process.Helpers;
using SOS.Process.Processors.DarwinCoreArchive;

namespace SOS.Process.IntegrationTests.TestHelpers
{
    public class DwcaObservationFactoryIntegrationFixture : TestBase, IDisposable
    {
        public DwcaObservationFactoryIntegrationFixture()
        {
            DwcaObservationFactory = CreateDwcaObservationFactoryAsync().Result;
        }

        public DwcaObservationFactory DwcaObservationFactory { get; private set; }

        public void Dispose()
        {
            DwcaObservationFactory = null;
        }

        private async Task<DwcaObservationFactory> CreateDwcaObservationFactoryAsync()
        {
            var dataProviderDummy = new DataProvider();
            var taxonByTaxonId = await GetTaxonDictionaryAsync();
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper =
                new AreaHelper(new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                    processedFieldMappingRepository);
            var dwcaObservationFactory = await DwcaObservationFactory.CreateAsync(
                dataProviderDummy,
                taxonByTaxonId,
                processedFieldMappingRepository,
                areaHelper);

            return dwcaObservationFactory;
        }

        private async Task<IDictionary<int, ProcessedTaxon>> GetTaxonDictionaryAsync()
        {
            var processedTaxonRepository = CreateProcessedTaxonRepository();
            var taxa = await processedTaxonRepository.GetAllAsync();
            return taxa.ToDictionary(taxon => taxon.Id, taxon => taxon);
        }

        private ProcessedTaxonRepository CreateProcessedTaxonRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            return new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessedTaxonRepository>());
        }
    }
}