using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.Harvesters.DwC;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class DwcDataStewardshipHarvesterIntegrationTests : TestBase
    {
        [Fact]
        public async Task Harvest_datastewardship_dwc_archive_with_taxalist_with_context()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-datastewardship-bats-taxalists.zip";
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
            IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath, @"C:\Temp\DwcaImport");
            var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
            var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
            var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var verbatimClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            DwcCollectionRepository dwcCollectionRepository = new DwcCollectionRepository(dataProvider, verbatimClient, new NullLogger<DwcCollectionRepository>());
            dwcCollectionRepository.BeginTempMode();
            await dwcCollectionRepository.DeleteCollectionsAsync();
            await dwcCollectionRepository.AddCollectionsAsync();
            await dwcCollectionRepository.OccurrenceRepository.AddManyAsync(occurrences);
            await dwcCollectionRepository.EventRepository.AddManyAsync(events.Cast<DwcEventOccurrenceVerbatim>());
            await dwcCollectionRepository.DatasetRepository.AddManyAsync(datasets);
            await dwcCollectionRepository.PermanentizeCollectionsAsync();
            dwcCollectionRepository.EndTempMode();

            var readOccurrences = await dwcCollectionRepository.OccurrenceRepository.GetAllAsync();
            var readDatasets = await dwcCollectionRepository.DatasetRepository.GetAllAsync();
            var readEvents = await dwcCollectionRepository.EventRepository.GetAllAsync();
            
            var dwcObservationHarvester = CreateDwcObservationHarvester();
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(
                archivePath,
                dataProvider,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            datasets.Should().NotBeNull();
            occurrences.Should().NotBeNull();
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Parse_Multiple_DwcaFiles()
        {
            foreach(var archivePath in _dwcaFiles)
            {
                await ParseDwcaFileAsync(archivePath);
            }            
        }

        private List<string> _dwcaFiles = new List<string>
        {            
            "./resources/dwca/dwca-datastewardship-bats-taxalists.zip",
            "./resources/dwca/dwca-event-emof-vims_neamap.zip",
            "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip",
            "./resources/dwca/dwca-occurrence-audubonmedia-rrel-cumv_amph.zip",
            "./resources/dwca/dwca-occurrence-emof-lifewatch-artportalen.zip",
            "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip"
        };


        private async Task ParseDwcaFileAsync(string archivePath)
        {
            try
            {
                var dataProvider = new DataProvider { Id = 200, Identifier = "TestDataset", Type = DataProviderType.DwcA };
                IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
                using var archiveReader = new ArchiveReader(archivePath, @"C:\Temp\DwcaImport");
                var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);
                var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
                var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
                var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Fact]
        public async Task Read_datastewardship_dwc_archive_with_taxalist_with_context()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-datastewardship-bats-taxalists.zip";
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
            IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath, @"C:\temp");
            var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
            var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
            var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            datasets.Should().NotBeNull();
            occurrences.Should().NotBeNull();
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task Read_datastewardship_dwc_archive_with_context()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-datastewardship-bats.zip";
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
            IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath, @"C:\temp");
            var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
            var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
            var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            datasets.Should().NotBeNull();
            occurrences.Should().NotBeNull();
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task Read_datastewardship_dwc_archive()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-datastewardship-bats.zip";
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
            IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath, @"C:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReader);
            var occurrences = await dwcArchiveReader.ReadArchiveAsync(archiveReader, dataProvider);
            var events = await dwcArchiveReader.ReadSamplingEventArchiveAsync(archiveReader, dataProvider);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            datasets.Should().NotBeNull();
            occurrences.Should().NotBeNull();
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task Harvest_datastewardship_dwc_archive()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-datastewardship-bats.zip";
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
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
                new DwcaConfiguration {ImportPath = @"C:\Temp", BatchSize=100},
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }
    }
}