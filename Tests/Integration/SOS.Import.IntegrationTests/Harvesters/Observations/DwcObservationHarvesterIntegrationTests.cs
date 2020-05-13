using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DwC_A.Terms;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Import.Harvesters.Observations;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class DwcObservationHarvesterIntegrationTests : TestBase
    {
        private const string SamplingEventDwcArchiveWithMofExtension = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";

        [Fact]
        public async Task Harvest_psophus_stridulus_occurrence_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-lifewatch-psophus-stridulus.zip";
            var datasetInfo = new DwcaDatasetInfo
            {
                DataProviderId = 100,
                DataProviderIdentifier = "TestPsophusStridulusCollection",
                ArchiveFilename = System.IO.Path.GetFileName(archivePath)
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                datasetInfo,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_occurrence_dwc_archive_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
            var datasetInfo = new DwcaDatasetInfo
            {
                DataProviderId = 101,
                DataProviderIdentifier = "TestLifeWatchSubsetCollection",
                ArchiveFilename = System.IO.Path.GetFileName(archivePath)
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath, 
                datasetInfo,
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
            var datasetInfo = new DwcaDatasetInfo
            {
                DataProviderId = 102,
                DataProviderIdentifier = "TestButterflyMonitoring",
                ArchiveFilename = System.IO.Path.GetFileName(archivePath)
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
                datasetInfo,
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
            var datasetInfo = new DwcaDatasetInfo
            {
                DataProviderId = 103,
                DataProviderIdentifier = "TestSHARK",
                ArchiveFilename = System.IO.Path.GetFileName(archivePath)
            };
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                datasetInfo,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }


        private DwcObservationHarvester CreateDwcObservationHarvester()
        {
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                importConfiguration.VerbatimDbConfiguration.BatchSize);
            var dwcObservationHarvester = new DwcObservationHarvester(
                new DarwinCoreArchiveVerbatimRepository(importClient, new NullLogger<DarwinCoreArchiveVerbatimRepository>()),
                new DarwinCoreArchiveEventRepository(importClient, new NullLogger<DarwinCoreArchiveEventRepository>()), 
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()), 
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }
    }
}