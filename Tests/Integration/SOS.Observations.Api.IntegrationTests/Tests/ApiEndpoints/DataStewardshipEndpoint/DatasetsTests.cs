using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.DataStewardship;
using SOS.Shared.Api.Dtos.Filter;

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
        var response = await apiClient.GetAsync($"/DataStewardship/datasets/{ArtportalenDatasetProjects.TreeDatasetIdentifier}");        
        var dataset = await response.Content.ReadFromJsonAsync<DsDatasetDto>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        var response = await apiClient.PostAsync($"/DataStewardship/datasets", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsDatasetDto>>(JsonSerializerOptions);

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
        var response = await apiClient.GetAsync($"/DataStewardship/occurrences/{occurrenceId}");
        var occurrence = await response.Content.ReadFromJsonAsync<DsOccurrenceDto>(JsonSerializerOptions);

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
        var response = await apiClient.PostAsync($"/DataStewardship/occurrences", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsOccurrenceDto>>(JsonSerializerOptions);

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
        var response = await apiClient.PostAsync($"/DataStewardship/events", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<DsEventDto>>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().BeGreaterThan(50, "Maximum is 60, but some observations could belong to the same event.");
    }
}