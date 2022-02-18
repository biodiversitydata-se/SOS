using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Managers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class DwcObservationHarvesterIntegrationTests : TestBase
    {
        [Fact]
        public async Task Harvest_occurrence_dwc_archive_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
            var dataProvider = new DataProvider
            {
                Id = 101,
                Identifier = "TestLifeWatchSubsetCollection",
                Type = DataProviderType.DwcA
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_psophus_stridulus_occurrence_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-lifewatch-psophus-stridulus.zip";
            var dataProvider = new DataProvider
            {
                Id = 100,
                Identifier = "TestPsophusStridulusCollection",
                Type = DataProviderType.DwcA
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_sampling_event_dwc_archive_with_mof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
            var dataProvider = new DataProvider
            {
                Id = 102,
                Identifier = "TestButterflyMonitoring",
                Type = DataProviderType.DwcA
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            // todo - handle sampling event based dwc and measurementOrFact extension.
            // https://github.com/gbif/ipt/wiki/BestPracticesSamplingEventData
            // https://www.gbif.org/data-quality-requirements-sampling-events
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_SHARK_dwca()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip";
            var dataProvider = new DataProvider
            {
                Id = 103,
                Identifier = "TestSHARK",
                Type = DataProviderType.DwcA
            };

            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_Riksskogstaxeringen()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = @"C:\DwC-A\Riksskogstaxeringen\Riksskogstaxeringen-RTFulldataset20200626.zip";

            if (!File.Exists(archivePath))
            {
                return;
            }

            var dataProvider = new DataProvider
            {
                Id = 104,
                Identifier = "Riksskogstaxeringen",
                Type = DataProviderType.DwcA
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }


        private DwcObservationHarvester CreateDwcObservationHarvester()
        {
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var processConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.GetMongoDbSettings(),
                processConfiguration.DatabaseName,
                processConfiguration.ReadBatchSize,
                processConfiguration.WriteBatchSize
            );

            var dwcObservationHarvester = new DwcObservationHarvester(
                importClient,                
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()),
                new FileDownloadService(new HttpClientService(new NullLogger<HttpClientService>()), new NullLogger<FileDownloadService>()),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()), 
                new DwcaConfiguration {ImportPath = @"C:\Temp"},
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }
    }
}