using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
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

        private DwcObservationHarvester CreateDwcObservationHarvester()
        {
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var dwcObservationHarvester = new DwcObservationHarvester(
                new DarwinCoreArchiveVerbatimRepository(importClient,
                    new NullLogger<DarwinCoreArchiveVerbatimRepository>()),
                new DarwinCoreArchiveEventRepository(importClient, new NullLogger<DarwinCoreArchiveEventRepository>()),
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()),
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }
    }
}