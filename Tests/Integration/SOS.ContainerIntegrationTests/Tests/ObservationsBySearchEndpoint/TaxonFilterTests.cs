using FizzWare.NBuilder;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for ObservationsBySearch endpoint when using taxon filters.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class TaxonFilterTests : IntegrationTestsBase
{
    public TaxonFilterTests(IntegrationTestsFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }
    
    [Fact]    
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByRedListCategories()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>
            .CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).HaveRedlistedTaxonId("CR") // Critically Endangered (Akut hotad)
             .TheNext(20).HaveRedlistedTaxonId("EN") // Endangered (Starkt hotad)
             .TheNext(20).HaveRedlistedTaxonId("VU") // Vulnerable (Sårbar)
             .TheNext(20).HaveRedlistedTaxonId("NT") // Near Threatened (Nära hotad)
             .TheNext(20).HaveRedlistedTaxonId(null) // Not redlisted taxa
            .Build();
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).With(o => o.TaxonId = TaxonIds.Skata)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Silvergran)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Talgoxe)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Blåmes)
            .Build();        
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonIds(TaxonIds.Silvergran, TaxonIds.Talgoxe, TaxonIds.Blåmes);

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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).With(o => o.TaxonId = TaxonIds.Svartmes)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Blåmes)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Tofsmes)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Duvhök)
            .Build();
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(20).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Asknätfjäril)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Sjögull)             
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jätteloka)
            .Build();
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge)
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
            .Build();
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(20).HaveTaxonCategoryTaxonId(17)
             .TheNext(20).HaveTaxonCategoryTaxonId(17)
             .TheNext(20).HaveTaxonCategoryTaxonId(18)
             .TheNext(20).HaveTaxonCategoryTaxonId(14)
             .TheNext(20).HaveTaxonCategoryTaxonId(11)             
            .Build();
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