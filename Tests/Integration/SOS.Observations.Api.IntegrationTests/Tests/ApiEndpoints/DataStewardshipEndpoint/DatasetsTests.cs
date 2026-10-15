using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Client.Clients;
using SOS.Lib.Extensions;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class DatasetsTests : ElasticsearchTestBase
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(4).With(m => m.Projects = null)
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
        result.Records!.Select(dataset => dataset.Identifier).Should().BeEquivalentTo(
            ArtportalenDatasetProjects.TreeDatasetIdentifier, ArtportalenDatasetProjects.UtterDatasetIdentifier);
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

        using var apiClient = TestFixture.CreateApiClient();
        string occurrenceId = processedObservations.First().Occurrence.OccurrenceId;
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        foreach (var responseCoordinateSystem in new[] { CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM })
        {
            // Act
            using var response = await apiClient.GetAsync($"/DataStewardship/occurrences/{occurrenceId}?responseCoordinateSystem={responseCoordinateSystem}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"responseCoordinateSystem={responseCoordinateSystem}");
            using var scope = new AssertionScope($"responseCoordinateSystem={responseCoordinateSystem}");
            var occurrence = await response.Content.ReadFromJsonAsync<DsOccurrenceDto>(jsonOptions, TestContext.Current.CancellationToken);
            occurrence!.OccurrenceID.Should().Be(occurrenceId);
            var expectedPoint = processedObservations.First().Location.Point.Transform(CoordinateSys.WGS84, responseCoordinateSystem);
            occurrence.ObservationPoint.Coordinates.Longitude.Should().BeApproximately(expectedPoint.X, 0.000001);
            occurrence.ObservationPoint.Coordinates.Latitude.Should().BeApproximately(expectedPoint.Y, 0.000001);
        }
    }


    [Fact]
    public async Task GetOccurrencesBySearchEndpoint_MatchesOccurrenceById_WithPagingAndCoordinateSystem()
    {
        var mapping = ArtportalenDatasetProjects.Mapping.Instance;
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
            .With(value => value.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project>
            {
                mapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId]
            })
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();
        var filter = new SearchFilterBaseDto
        {
            DataStewardship = new DataStewardshipFilterDto
            {
                DatasetIdentifiers = [ArtportalenDatasetProjects.TreeDatasetIdentifier]
            }
        };

        foreach (var responseCoordinateSystem in new[] { CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM })
        {
            using var searchResponse = await client.PostAsync(
                $"/DataStewardship/occurrences?skip=1&take=1&responseCoordinateSystem={responseCoordinateSystem}",
                JsonContent.Create(filter), TestContext.Current.CancellationToken);

            searchResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"responseCoordinateSystem={responseCoordinateSystem}");
            using var scope = new AssertionScope($"responseCoordinateSystem={responseCoordinateSystem}");
            using var page = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            page.RootElement.GetProperty("skip").GetInt32().Should().Be(1);
            page.RootElement.GetProperty("take").GetInt32().Should().Be(1);
            page.RootElement.GetProperty("totalCount").GetInt64().Should().Be(2);
            var record = page.RootElement.GetProperty("records").EnumerateArray().Should().ContainSingle().Which;
            var occurrenceId = record.GetProperty("occurrenceID").GetString()!;
            using var getResponse = await client.GetAsync(
                $"/DataStewardship/occurrences/{Uri.EscapeDataString(occurrenceId)}?responseCoordinateSystem={responseCoordinateSystem}",
                TestContext.Current.CancellationToken);

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            using var occurrence = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            JsonElement.DeepEquals(occurrence.RootElement, record).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetOccurrencesBySearchEndpoint_ReturnsExpectedOccurrences_WhenSearchingObservationsByDatasetIdentifiers()
    {
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(4).With(m => m.Projects = null)
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
        result!.TotalCount.Should().Be(6);
        result.Records!.Select(occurrence => occurrence.OccurrenceID).Should().BeEquivalentTo(
            verbatimObservations.Take(6).Select(observation => $"urn:lsid:artportalen.se:sighting:{observation.SightingId}"));
    }


    [Fact]
    public async Task GetEventsBySearchEndpoint_ReturnsExpectedEvents_WhenSearchingEventsByDatasetIdentifiers()
    {
        // Arrange        
        var apDatasetMapping = ArtportalenDatasetProjects.Mapping.Instance;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
                .With(observation => observation.Site.Id = 4060923)
                .With(observation => observation.StartDate = new DateTime(2000, 1, 1))
                .With(observation => observation.EndDate = new DateTime(2000, 1, 1))
                .With(observation => observation.StartTime = null)
                .With(observation => observation.EndTime = null)
            .TheFirst(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.TreeProjectId] })
             .TheNext(3).With(m => m.Projects = new List<SOS.Lib.Models.Verbatim.Artportalen.Project> { apDatasetMapping.ProjectById[ArtportalenDatasetProjects.UtterProjectId] })
             .TheNext(4).With(m => m.Projects = null)
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
        result!.TotalCount.Should().Be(2, "observations at the same site and date are grouped into one event per dataset");
        result.Records!.Select(ev => ev.EventID).Should().BeEquivalentTo(
            processedObservations.Where(observation => observation.DataStewardship != null)
                .Select(observation => observation.Event.EventId).Distinct());
        result.Records!.Select(ev => ev.Dataset.Identifier).Should().BeEquivalentTo(
            ArtportalenDatasetProjects.TreeDatasetIdentifier, ArtportalenDatasetProjects.UtterDatasetIdentifier);
    }
}
