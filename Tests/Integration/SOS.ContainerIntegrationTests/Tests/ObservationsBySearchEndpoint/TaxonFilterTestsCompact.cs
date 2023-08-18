using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for ObservationsBySearch endpoint when using taxon filters.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class TaxonFilterTestsCompact : IntegrationTestsBase
{
    public TaxonFilterTestsCompact(IntegrationTestsFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }
    
    [Fact]    
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByRedListCategories()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithRedlistCategories("CR", "EN", "VU", "NT", null);        
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithRedListCategories("CR", "EN", "VU");

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 out of 100 observations added to Elasticsearch had one of the Red List categories CR, EN, or VU.");
    }
   
    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByTaxonIds()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithTaxonIds(
            TaxonIds.Jättebalsamin, TaxonIds.Silvergran, TaxonIds.Blåmes, TaxonIds.Talgoxe, TaxonIds.Skata);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonIds(TaxonIds.Silvergran, TaxonIds.Blåmes, TaxonIds.Talgoxe);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 out of 100 observations added to Elasticsearch had one of the Taxon Silvergran, Blåmes, Talgoxe.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForUnderlyingTaxa()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithTaxonIds(
            TaxonIds.Svartmes, TaxonIds.Blåmes, TaxonIds.Tofsmes, TaxonIds.Duvhök, TaxonIds.Jättebalsamin);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonIds(includeUnderlyingTaxa: true, TaxonIds.Mesar);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 out of 100 observations added to Elasticsearch has Taxon Mesar as ancestor.");
    }      

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchByTaxonListMerge()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithTaxonIds(
            TaxonIds.SvartfläckigBlåvinge, TaxonIds.Asknätfjäril, TaxonIds.Jättebalsamin, TaxonIds.Sjögull, TaxonIds.Jätteloka);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonListId(
            (int)TaxonListId.ProtectedByLaw, TaxonListOperatorDto.Merge, TaxonIds.Jättebalsamin);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "Svartfläckig blåvinge (20 records) and Asknätfjäril (20 records) is protected by law " +
                     "and Jättebalsamin (20 records) is merged with the taxa list.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchByTaxonListFilter()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithTaxonIds(TaxonIds.ViolettGuldvinge, 
            TaxonIds.ViolettGuldvinge, TaxonIds.ViolettGuldvinge, TaxonIds.SvartfläckigBlåvinge, TaxonIds.Jättebalsamin);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonListId(
            (int)TaxonListId.ProtectedByLaw, TaxonListOperatorDto.Filter, TaxonIds.ViolettGuldvinge, TaxonIds.Jättebalsamin);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "Violett guldvinge (60 records) is protected by law but not Jättebalsamin.");
    }
  
    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilterByTaxonCategories()
    {
        // Arrange        
        var verbatimObservations = ArtportalenTestData.Create100RecordsWithTaxonCategoryIds(17, 17, 18, 14, 11);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonCategories(17, 18);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 record has taxon category 17 or 18.");
    }
}