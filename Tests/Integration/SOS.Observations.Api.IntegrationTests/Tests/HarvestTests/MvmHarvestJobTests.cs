using Hangfire;
using SOS.Harvest.Harvesters.Mvm;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Processors.Mvm;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Tests.HarvestTests;

/// <summary>
/// Integration tests for MVM harvest observations.
/// </summary>
[Collection(TestCollection.Name)]
public class MvmHarvestJobTests : ElasticsearchTestBase
{
    public MvmHarvestJobTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact(Skip = "Run only on demand")]    
    public async Task HarvestMvm_ShouldHaveExpectedRecords()
    {
        // You need to change MvmObservationHarvester to read from an example file.

        // Arrange
        DataProvider dataProvider = new DataProvider() { Id = 4 };
        MvmObservationHarvester mvmObservationHarvester = TestFixture.ServiceProvider.GetService<MvmObservationHarvester>()!;        
        IMvmObservationProcessor mvmObservationProcessor = TestFixture.ServiceProvider.GetService<IMvmObservationProcessor>()!;
        IProcessedObservationRepository processedObservationRepository = TestFixture.ServiceProvider.GetService<IProcessedObservationRepository>()!;
        await TestFixture.InitializeAreasAsync();

        // Act        
        await mvmObservationHarvester.HarvestObservationsAsync(dataProvider, JobCancellationToken.Null);
        await mvmObservationProcessor.ProcessAsync(dataProvider, TestFixture.ProcessFixture.TaxonById, TestFixture.ProcessFixture.DwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);
        await processedObservationRepository.WaitForPublicIndexCreationAsync(15, TimeSpan.FromSeconds(15));
        var observations = await processedObservationRepository.GetAllAsync();

        // Assert
        observations.Count.Should().BeGreaterThan(100, because: "The sample file contains more than 100 observations");
    }
}