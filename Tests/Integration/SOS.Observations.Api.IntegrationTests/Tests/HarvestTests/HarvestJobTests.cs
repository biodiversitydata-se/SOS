using Hangfire;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Verbatim;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Tests.HarvestTests;

/// <summary>
/// Integration tests for harvest observations.
/// </summary>
[Collection(TestCollection.Name)]
public class HarvestJobTests : TestBase
{
    public HarvestJobTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task HarvestDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingSingleDataset()
    {
        // Arrange
        IDwcObservationHarvester dwcObservationHarvester = TestFixture.ServiceProvider.GetService<IDwcObservationHarvester>()!;
        DataProvider dataProvider = new DataProvider() { Id = 13, Identifier = "test" };

        // Act
        await dwcObservationHarvester.HarvestObservationsAsync(@"Resources/Dwca/dwca-datastewardship-single-dataset.zip", dataProvider, JobCancellationToken.Null);
        var dwcaData = await ReadDwcaDataAsync(dataProvider);

        // Assert
        dwcaData?.Datasets?.Count.Should().Be(1, because: "the DwC-A file contains 1 dataset");
        dwcaData?.Events?.Count.Should().Be(7, because: "the DwC-A file contains 7 events");
        dwcaData?.Occurrences?.Count.Should().Be(15, because: "the DwC-A file contains 15 occurrences");
    }

    [Fact]
    public async Task HarvestAndProcessDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingSingleDataset()
    {
        // Arrange
        DataProvider dataProvider = new DataProvider() { Id = 13 };
        IDwcObservationHarvester dwcObservationHarvester = TestFixture.ServiceProvider.GetService<IDwcObservationHarvester>()!;
        IDwcaObservationProcessor dwcaObservationProcessor = TestFixture.ServiceProvider.GetService<IDwcaObservationProcessor>()!;
        IProcessedObservationRepository processedObservationRepository = TestFixture.ServiceProvider.GetService<IProcessedObservationRepository>()!;
        await TestFixture.InitializeAreasAsync();

        // Act
        await dwcObservationHarvester.HarvestObservationsAsync(@"Resources/Dwca/dwca-datastewardship-single-dataset.zip", dataProvider, JobCancellationToken.Null);
        await dwcaObservationProcessor.ProcessAsync(dataProvider, TestFixture.ProcessFixture.TaxonById, TestFixture.ProcessFixture.DwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);
        await processedObservationRepository.WaitForPublicIndexCreationAsync(15, TimeSpan.FromSeconds(15));
        var observations = await processedObservationRepository.GetAllAsync();

        // Assert
        observations.Count.Should().Be(15, because: "the DwC-A file contains 15 occurrences");
    }

    private async Task<DwcaData> ReadDwcaDataAsync(DataProvider dataProvider)
    {
        IVerbatimClient verbatimClient = TestFixture.ServiceProvider.GetService<IVerbatimClient>()!;
        using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, verbatimClient, new NullLogger<DwcCollectionRepository>());
        var occurrences = await dwcCollectionRepository.OccurrenceRepository.GetAllAsync();
        var events = await dwcCollectionRepository.EventRepository.GetAllAsync();
        var datasets = await dwcCollectionRepository.DatasetRepository.GetAllAsync();

        return new DwcaData
        {
            Datasets = datasets,
            Events = events,
            Occurrences = occurrences
        };
    }

    private class DwcaData
    {
        public List<DwcVerbatimDataset>? Datasets { get; set; }
        public List<DwcEventOccurrenceVerbatim>? Events { get; set; }
        public List<DwcObservationVerbatim>? Occurrences { get; set; }
    }
}