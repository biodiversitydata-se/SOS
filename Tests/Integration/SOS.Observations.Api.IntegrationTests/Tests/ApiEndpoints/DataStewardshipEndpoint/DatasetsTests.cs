using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Dtos;
using SOS.Lib.Dtos.DataStewardship;
using SOS.Lib.Dtos.Filter;
using SOS.Api.Client.Clients;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class DatasetsTests : TestBase
{
    public DatasetsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task GetDatasetByIdEndpoint_ReturnsExpectedDataset_WhenRouteContainsDatasetIdentifier()
    {        
        // Arrange
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimDataset = apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier];
        var processRes = await ProcessFixture.ProcessAndAddDatasetsToElasticSearch(new List<ArtportalenDatasetMetadata> { verbatimDataset });                
        var apiClient = TestFixture.CreateApiClient();        

        // Act
        var response = await apiClient.GetAsync($"/DataStewardship/datasets/{ArtportalenDatasetProjects.TreeDatasetIdentifier}", TestContext.Current.CancellationToken);        
        var dataset = await response.Content.ReadFromJsonAsync<DsDatasetDto>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dataset!.Identifier.Should().Be(ArtportalenDatasetProjects.TreeDatasetIdentifier);
    }

    [Fact]
    public async Task SosDataStewardshipClient_GetDatasetAsync_UsesObservationApiPipeline()
    {
        var mapping = ArtportalenDatasetProjects.Mapping.Instance;
        var source = mapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier];
        await ProcessFixture.ProcessAndAddDatasetsToElasticSearch([source]);
        var client = new SosDataStewardshipClient(TestFixture.CreateApiClient());

        var dataset = await client.GetDatasetAsync(
            ArtportalenDatasetProjects.TreeDatasetIdentifier,
            cancellationToken: TestContext.Current.CancellationToken);

        dataset.Should().NotBeNull();
        dataset!.Identifier.Should().Be(ArtportalenDatasetProjects.TreeDatasetIdentifier);
    }

    [Fact]
    public async Task GetDatasetsBySearchEndpoint_ReturnsExpectedDatasets_WhenSearchingObservationsByDatasetIdentifiers()
    {
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(40).With(m => m.Projects = null)             
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        
        List<ArtportalenDatasetMetadata> verbatimDatasets = [
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier],
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.UtterDatasetIdentifier]
            ];
        await ProcessFixture.ProcessAndAddDatasetsToElasticSearch(verbatimDatasets);
        
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterBaseDto()
        {
            DataStewardship = new DataStewardshipFilterDto
            {
                DatasetIdentifiers = new List<string> 
                {
                    ArtportalenDatasetProjects.TreeDatasetIdentifier,
                    ArtportalenDatasetProjects.UtterDatasetIdentifier
                }
            }
        };        

        // Act
        var response = await apiClient.PostAsync($"/DataStewardship/datasets", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsDatasetDto>>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2);        
    }

    [Fact]
    public async Task GetOccurrenceByIdEndpoint_ReturnsExpectedOccurrence_WhenRouteContainsExistingObservation()
    {        
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(1)
            .All().HaveValuesFromPredefinedObservations()
                .With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })             
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

        List<ArtportalenDatasetMetadata> verbatimDatasets = [
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier],
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.UtterDatasetIdentifier]
            ];
        await ProcessFixture.ProcessAndAddDatasetsToElasticSearch(verbatimDatasets);

        var apiClient = TestFixture.CreateApiClient();
        string occurrenceId = processedObservations.First().Occurrence.OccurrenceId;

        // Act
        var response = await apiClient.GetAsync($"/DataStewardship/occurrences/{occurrenceId}", TestContext.Current.CancellationToken);
        var occurrence = await response.Content.ReadFromJsonAsync<DsOccurrenceDto>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        occurrence!.OccurrenceID.Should().Be(occurrenceId);
    }


    [Fact]
    public async Task GetOccurrencesBySearchEndpoint_ReturnsExpectedOccurrences_WhenSearchingObservationsByDatasetIdentifiers()
    {
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(40).With(m => m.Projects = null)
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

        List<ArtportalenDatasetMetadata> verbatimDatasets = [
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier],
            apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.UtterDatasetIdentifier]
            ];
        await ProcessFixture.ProcessAndAddDatasetsToElasticSearch(verbatimDatasets);

        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterBaseDto()
        {
            DataStewardship = new DataStewardshipFilterDto
            {
                DatasetIdentifiers = new List<string>
                {
                    ArtportalenDatasetProjects.TreeDatasetIdentifier,
                    ArtportalenDatasetProjects.UtterDatasetIdentifier
                }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/DataStewardship/occurrences", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsOccurrenceDto>>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60);
    }


    [Fact]
    public async Task GetEventsBySearchEndpoint_ReturnsExpectedEvents_WhenSearchingEventsByDatasetIdentifiers()
    {
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(30).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(40).With(m => m.Projects = null)
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

        List<ArtportalenDatasetMetadata> verbatimDatasets = [
                apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.TreeDatasetIdentifier],
                apDatasetMapping.DatasetByIdentifier[ArtportalenDatasetProjects.UtterDatasetIdentifier]
            ];
        await ProcessFixture.ProcessAndAddDatasetsToElasticSearch(verbatimDatasets);
        await ProcessFixture.ProcessAndAddArtportalenEventsToElasticSearch();

        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterBaseDto()
        {
            DataStewardship = new DataStewardshipFilterDto
            {
                DatasetIdentifiers = new List<string>
                {
                    ArtportalenDatasetProjects.TreeDatasetIdentifier,
                    ArtportalenDatasetProjects.UtterDatasetIdentifier
                }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/DataStewardship/events", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsEventDto>>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().BeGreaterThan(50, "Maximum is 60, but some observations could belong to the same event.");
    }
}
