﻿using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Process.LiveIntegrationTests.TestHelpers
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
            var invalidEventRepositoryMock = new Mock<IInvalidEventRepository>();
            ValidationManager validationManager = new ValidationManager(invalidObservationRepositoryMock.Object,
                invalidEventRepositoryMock.Object,
                new NullLogger<ValidationManager>());
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
            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var areaHelper =
                new AreaHelper(new AreaConfiguration(), new AreaRepository(processClient, new NullLogger<AreaRepository>()));
            var processConfiguration = new ProcessConfiguration();
            var dwcaObservationFactory = await DwcaObservationFactory.CreateAsync(
                dataProviderDummy,
                taxonByTaxonId,
                vocabularyRepository,
                areaHelper,
                new ProcessTimeManager(processConfiguration),
                processConfiguration);

            return dwcaObservationFactory;
        }

        private async Task<IDictionary<int, Taxon>> GetTaxonDictionaryAsync()
        {
            var processedTaxonRepository = CreateProcessedTaxonRepository();
            var taxa = await processedTaxonRepository.GetAllAsync();
            return taxa.ToDictionary(taxon => taxon.Id, taxon => taxon);
        }

        private TaxonRepository CreateProcessedTaxonRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            return new TaxonRepository(
                processClient,
                new NullLogger<TaxonRepository>());
        }
    }
}