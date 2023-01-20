using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DwcaImportTests : TestBase
{
    private readonly ProcessFixture _processFixture;
    private readonly ITestOutputHelper output;

    public DwcaImportTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory, 
        ITestOutputHelper output) : base(webApplicationFactory) 
    {
        this.output = output;
        using var scope = _factory.ServiceProvider.CreateScope();        
        _processFixture = scope.ServiceProvider.GetService<ProcessFixture>();
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_observations()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(@"resources\dwca-datastewardship-bats-taxalists.zip");
        var dwcFactory = _processFixture.GetDarwinCoreFactory(true);       
        var processedObservations = parsedDwcaFile
            .Occurrences
            .Select(m => dwcFactory.CreateProcessedObservation(m, false))
            .ToList();
        output.WriteLine($"Processed observations count= {processedObservations.Count}");
        await AddObservationsToElasticsearchAsync(processedObservations);
        
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var observationById1 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571703", jsonSerializerOptions);
        var observationById2 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571253", jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Hallaröd)"
        var searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };        
        var pageResultHallarod = await Client.PostAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Other)"
        searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await Client.PostAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        observationById1.Should().NotBeNull();
        observationById2.Should().NotBeNull();

        pageResultHallarod.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResultOther.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }
}