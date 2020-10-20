using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Process.Processors.DarwinCoreArchive;

namespace SOS.Process.IntegrationTests.TestHelpers
{
    public class DwcaObservationFactoryIntegrationFixture : TestBase, IDisposable
    {
        public DwcaObservationFactoryIntegrationFixture()
        {
            DwcaObservationFactory = CreateDwcaObservationFactoryAsync().Result;
            ValidationManager = CreateValidationManager();
        }

        public DwcaObservationFactory DwcaObservationFactory { get; private set; }
        public ValidationManager ValidationManager { get; private set; }

        public void Dispose()
        {
            DwcaObservationFactory = null;
        }

        private ValidationManager CreateValidationManager()
        {
            var invalidObservationRepositoryMock = new Mock<IInvalidObservationRepository>();
            ValidationManager validationManager = new ValidationManager(invalidObservationRepositoryMock.Object, new NullLogger<ValidationManager>());
            return validationManager;
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
                new FieldMappingRepository(processClient, new NullLogger<FieldMappingRepository>());
            var areaHelper =
                new AreaHelper(new AreaRepository(processClient, new NullLogger<AreaRepository>()),
                    processedFieldMappingRepository);
            var dwcaObservationFactory = await DwcaObservationFactory.CreateAsync(
                dataProviderDummy,
                taxonByTaxonId,
                processedFieldMappingRepository,
                areaHelper);

            return dwcaObservationFactory;
        }

        private async Task<IDictionary<int, Taxon>> GetTaxonDictionaryAsync()
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