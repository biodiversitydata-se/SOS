using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ExportsEndpoints;

[Collection(TestCollection.Name)]
public class DownloadCsvTests : TestBase
{
    public DownloadCsvTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DownloadCsvFileEndpoint_ReturnsExpectedRows_WhenNoFilterIsUsed()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/csv?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = CsvHelper.ReadCsvFile(contentBytes);
        fileEntries.Count.Should().Be(100);
    }

    [Fact]
    public async Task DownloadCsvFileEndpoint_ReturnsExpectedRowsAndColumns_WhenOutputFieldsAreSpecified()
    {
        // Arrange
        const int sightingId = 123456;
        const string occurrenceId = "urn:lsid:artportalen.se:sighting:123456";
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(m => m.SightingId = sightingId)
                        .With(m => m.DatasourceId = 1)
                        .With(m => m.Observers = "Tom Volgers")
                        .With(m => m.Activity = new MetadataWithCategory<int>((int)ActivityId.Incubating, 1))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Output = new OutputFilterDto
            {
                Fields = new List<string> { "Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity" }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/csv?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = CsvHelper.ReadCsvFile(contentBytes);
        fileEntries.Count.Should().Be(100);
        var fileEntry = fileEntries.Single(m => m["OccurrenceId"] == occurrenceId);
        fileEntry["DatasetName"].Should().Be("Artportalen");
        fileEntry["RecordedBy"].Should().Be("Tom Volgers");
        fileEntry["Activity"].Should().Be("ruvande");
    }

    [Fact]
    public async Task DownloadCsvFileEndpoint_ReturnsExpectedColumnNames_WhenPropertyLabelTypeIsSpecified()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Output = new OutputFilterDto
            {
                Fields = new List<string> { "Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity" }
            }
        };

        // Act
        var propertyNameResponse = await apiClient.PostAsync($"/exports/download/csv?propertyLabelType=propertyname&gzip=false", JsonContent.Create(searchFilter));
        byte[] propertyNameFileBytes = await propertyNameResponse.Content.ReadAsByteArrayAsync();
        var propertyPathResponse = await apiClient.PostAsync($"/exports/download/csv?propertyLabelType=propertypath&gzip=false", JsonContent.Create(searchFilter));
        byte[] propertyPathFileBytes = await propertyPathResponse.Content.ReadAsByteArrayAsync();
        var swedishResponse = await apiClient.PostAsync($"/exports/download/csv?propertyLabelType=swedish&gzip=false", JsonContent.Create(searchFilter));
        byte[] swedishFileBytes = await swedishResponse.Content.ReadAsByteArrayAsync();
        var englishResponse = await apiClient.PostAsync($"/exports/download/csv?propertyLabelType=english&gzip=false", JsonContent.Create(searchFilter));
        byte[] englishFileBytes = await englishResponse.Content.ReadAsByteArrayAsync();

        // Assert
        var propertyNameEntries = CsvHelper.ReadCsvFile(propertyNameFileBytes);
        var propertyPathEntries = CsvHelper.ReadCsvFile(propertyPathFileBytes);
        var swedishEntries = CsvHelper.ReadCsvFile(swedishFileBytes);
        var englishEntries = CsvHelper.ReadCsvFile(englishFileBytes);
        propertyNameEntries.First().Keys.Should()
            .BeEquivalentTo("OccurrenceId", "DatasetName", "RecordedBy", "Activity");
        propertyPathEntries.First().Keys.Should()
            .BeEquivalentTo("Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity");
        swedishEntries.First().Keys.Should()
            .BeEquivalentTo("Observation GUID", "Datakälla", "Observatör", "Aktivitet");
        englishEntries.First().Keys.Should()
            .BeEquivalentTo("Occurrence Id", "Dataset", "Recorded By", "Activity");
    }

    [Fact]
    public async Task DownloadCsvFileEndpoint_ContainsProjectAndMediaColumns_WhenProjectAndMediaFieldsAreIncludedInFilter()
    {
        // Arrange
        const int sightingId = 123456;
        const string occurrenceId = "urn:lsid:artportalen.se:sighting:123456";
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(m => m.SightingId = sightingId)
                        .HaveProjectInformation()
                        .HaveMediaInformation()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Output = new OutputFilterDto
            {
                FieldSet = OutputFieldSet.Extended,
                Fields = new List<string> { "Occurrence.Media" }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/csv?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = CsvHelper.ReadCsvFile(contentBytes);
        var fileEntry = fileEntries.Single(m => m["OccurrenceId"] == occurrenceId);
        fileEntry["Projects"].Should().NotBeNullOrEmpty();
        fileEntry["Media"].Should().NotBeNullOrEmpty();
    }
}