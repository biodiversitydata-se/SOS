using Hangfire;
using SOS.Harvest.Harvesters.VirtualHerbarium;
using SOS.Harvest.Processors.VirtualHerbarium;
using SOS.Harvest.Processors.VirtualHerbarium.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.HarvestTests;

/// <summary>
/// Integration tests for Virtual herbarium harvest observations.
/// </summary>
[Collection(TestCollection.Name)]
public class VirtualHerbariumHarvestJobTests : ElasticsearchTestBase
{
    public VirtualHerbariumHarvestJobTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact(Skip = "Run only on demand")]    
    public async Task HarvestVirtualHerbarium_ShouldHaveExpectedRecords()
    {
        // Arrange
        DataProvider dataProvider = new DataProvider() { Id = 7 };
        var virtualHerbariumObservationHarvester = TestFixture.ServiceProvider.GetService<VirtualHerbariumObservationHarvester>()!;
        var virtualHerbariumObservationProcessor = TestFixture.ServiceProvider.GetService<VirtualHerbariumObservationProcessor>()!;
        Lib.Repositories.Processed.Interfaces.IProcessedObservationCoreRepository processedObservationRepository = TestFixture.ServiceProvider.GetService<Lib.Repositories.Processed.Interfaces.IProcessedObservationCoreRepository>()!;
        await TestFixture.InitializeAreasAsync();

        // Act
        await virtualHerbariumObservationHarvester.HarvestObservationsAsync(dataProvider, JobCancellationToken.Null);
        await virtualHerbariumObservationProcessor.ProcessAsync(dataProvider, TestFixture.ProcessFixture.TaxonById, TestFixture.ProcessFixture.DwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);
        await processedObservationRepository.WaitForPublicIndexCreationAsync(15, TimeSpan.FromSeconds(15));
        var observations = await processedObservationRepository.GetAllAsync();

        // Assert
        observations.Count.Should().BeGreaterThan(20, because: "20 of the 100 first observations harvested is successfully processed.");
    }
}