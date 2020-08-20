using System.IO;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver.Core.Operations;
using Moq;
using Newtonsoft.Json;
using SOS.Import.DarwinCore;
using SOS.Import.Managers;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Json;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Managers;
using Xunit;

namespace SOS.Import.IntegrationTests.Managers
{
    public class DwcaDataValidationReportManagerTests : TestBase
    {
        private FieldMappingResolverHelper CreateFieldMappingResolverHelper()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            return new FieldMappingResolverHelper(processedFieldMappingRepository,
                new FieldMappingConfiguration {LocalizationCultureCode = "sv-SE", ResolveValues = true});
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
                .Ignore<ProcessedObservation>(obs => obs.Location.Point)
                .Ignore<ProcessedObservation>(obs => obs.Location.PointWithBuffer)
                .Ignore<ProcessedObservation>(obs => obs.IsInEconomicZoneOfSweden)
                .Ignore<DwcObservationVerbatim>(obs => obs.RecordId)
                .Ignore<DwcObservationVerbatim>(obs => obs.Id)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderId)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderIdentifier)
                .KeepTypeWithDefaultValue(typeof(ProcessedFieldMapValue));

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
            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper = new AreaHelper(new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                    processedFieldMappingRepository);
            var fieldMappingResolverHelper = new FieldMappingResolverHelper(processedFieldMappingRepository,
                new FieldMappingConfiguration { LocalizationCultureCode = "sv-SE", ResolveValues = true });
            var processedTaxonRepository = new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessedTaxonRepository>());
            var validationReportManager = new DwcaDataValidationReportManager(
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()),
                processedFieldMappingRepository,
                CreateValidationManager(),
                areaHelper,
                fieldMappingResolverHelper,
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