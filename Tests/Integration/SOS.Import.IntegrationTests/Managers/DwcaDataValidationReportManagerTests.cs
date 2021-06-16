using System.IO;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using SOS.Import.DarwinCore;
using SOS.Import.Managers;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Json;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Import.IntegrationTests.Managers
{
    public class DwcaDataValidationReportManagerTests : TestBase
    {
        private VocabularyValueResolver CreateVocabularyValueResolver()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            return new VocabularyValueResolver(vocabularyRepository,
                new VocabularyConfiguration {LocalizationCultureCode = "sv-SE", ResolveValues = true});
        }

        [Fact]
        public async Task Compare_SHARK_verbatim_and_processed_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip";
            const string savePath = @"c:\temp\DwcaDataValidationReport-SHARK.json";
            var validationReportManager = CreateDwcaDataValidationReportManager();
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await validationReportManager.CreateDataValidationSummary(archiveReader);
            var jsonSettings = CreateJsonSerializerSettings();
            var strJsonObservationCompare = JsonConvert.SerializeObject(result, Formatting.Indented, jsonSettings);
            if (File.Exists(savePath)) File.Delete(savePath);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Compare_Riksskogstaxeringen_verbatim_and_processed_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = @"C:\DwC-A\Riksskogstaxeringen\Riksskogstaxeringen-RTFulldataset20200626.zip";
            const string savePath = @"C:\Temp\DwcaDataValidationReport-Riksskogstaxeringen.json";
            var validationReportManager = CreateDwcaDataValidationReportManager();
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await validationReportManager.CreateDataValidationSummary(archiveReader);
            var jsonSettings = CreateJsonSerializerSettings();
            var strJsonObservationCompare = JsonConvert.SerializeObject(result, Formatting.Indented, jsonSettings);
            if (File.Exists(savePath)) File.Delete(savePath);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Compare_SwedishButterflyMonitoring_verbatim_and_processed_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
            const string savePath = @"c:\temp\DwcaDataValidationReport-SwedishButterflyMonitoring.json";
            var validationReportManager = CreateDwcaDataValidationReportManager();
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await validationReportManager.CreateDataValidationSummary(archiveReader);
            var jsonSettings = CreateJsonSerializerSettings();
            var strJsonObservationCompare = JsonConvert.SerializeObject(result, Formatting.Indented, jsonSettings);
            if (File.Exists(savePath)) File.Delete(savePath);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        private JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver { SetStringPropertyDefaultsToEmptyString = true}
                .Ignore<Observation>(obs => obs.Location.Point)
                .Ignore<Observation>(obs => obs.Location.PointWithBuffer)
                .Ignore<Observation>(obs => obs.IsInEconomicZoneOfSweden)
                .Ignore<DwcObservationVerbatim>(obs => obs.RecordId)
                .Ignore<DwcObservationVerbatim>(obs => obs.Id)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderId)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderIdentifier)
                .KeepTypeWithDefaultValue(typeof(VocabularyValue));

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            return jsonSettings;
        }

        private static ProcessClient CreateProcessClient(MongoDbConfiguration mongoDbConfiguration)
        {
            var processClient = new ProcessClient(
                mongoDbConfiguration.GetMongoDbSettings(),
                mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize,
                mongoDbConfiguration.WriteBatchSize);
            
            return processClient;
        }

        private DwcaDataValidationReportManager CreateDwcaDataValidationReportManager()
        {
            var processClient = CreateProcessClient(GetProcessDbConfiguration());
            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var areaHelper = new AreaHelper(new AreaRepository(processClient, new NullLogger<AreaRepository>()));
            var vocabularyValueResolver = new VocabularyValueResolver(vocabularyRepository,
                new VocabularyConfiguration { LocalizationCultureCode = "sv-SE", ResolveValues = true });
            var processedTaxonRepository = new TaxonRepository(
                processClient,
                new NullLogger<TaxonRepository>());

            var validationReportManager = new DwcaDataValidationReportManager(
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()),
                vocabularyRepository,
                CreateValidationManager(),
                areaHelper,
                vocabularyValueResolver,
                processedTaxonRepository,
                new NullLogger<DwcaDataValidationReportManager>()
            );

            return validationReportManager;
        }

        private ValidationManager CreateValidationManager()
        {
            var invalidObservationRepositoryMock = new Mock<IInvalidObservationRepository>();
            ValidationManager validationManager = new ValidationManager(invalidObservationRepositoryMock.Object, new NullLogger<ValidationManager>());
            return validationManager;
        }
    }
}