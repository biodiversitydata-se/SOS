using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Repositories.Destination;

namespace SOS.Process.IntegrationTests.TestHelpers
{
    public class DwcaObservationFactoryIntegrationFixture : TestBase, IDisposable
    {
        public DwcaObservationFactory DwcaObservationFactory { get; private set; }

        public DwcaObservationFactoryIntegrationFixture()
        {
            DwcaObservationFactory = CreateDwcaObservationFactoryAsync().Result;
        }

        public void Dispose()
        {
            DwcaObservationFactory = null;
        }

        private async Task<DwcaObservationFactory> CreateDwcaObservationFactoryAsync()
        {
            var taxonByTaxonId = await GetTaxonDictionaryAsync();
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper =
                new AreaHelper(new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                    processedFieldMappingRepository);
            var dwcaObservationFactory = await DwcaObservationFactory.CreateAsync(
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
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            return new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessedTaxonRepository>());
        }
    }

}
