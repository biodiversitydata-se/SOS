using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.Factories;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for ObservationsBySearch endpoint when using taxon filters.
/// </summary>
[Collection(TestCollection.Name)]
public class TaxonFilterTests : TestBase
{
    public TaxonFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByRedListCategories()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>
            .CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveRedlistedTaxonId("CR") // Critically Endangered (Akut hotad)
             .TheNext(1).HaveRedlistedTaxonId("EN") // Endangered (Starkt hotad)
             .TheNext(1).HaveRedlistedTaxonId("VU") // Vulnerable (Sårbar)
             .TheNext(1).HaveRedlistedTaxonId("NT") // Near Threatened (Nära hotad)
             .TheNext(1).HaveRedlistedTaxonId(null) // Not redlisted taxa
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                RedListCategories = new string[] { "CR", "EN", "VU" }
            }
        };
                // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var debug = DebugTestOnlyRedlistCategoryFilter(verbatimObservations, processedObservations, result!.Records);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 out of 5 observations added to Elasticsearch had one of the Red List categories CR, EN, or VU.");
    }

    private List<TestResultItem> DebugTestOnlyRedlistCategoryFilter(
    IList<ArtportalenObservationVerbatim> verbatimObservations,
    IEnumerable<Observation> allObservations,
    IEnumerable<Observation> resultObservations)
    {
        var testResultItems = DebugTestResultHelper.CreateTestResultSummary(verbatimObservations, allObservations, resultObservations);
        foreach (var item in testResultItems)
        {
            item.VerbatimValue = item.VerbatimObservation?.TaxonId!.Value;
            item.ProcessedValue = item.ProcessedObservation?.Taxon!.Attributes.RedlistCategory;
        }

        testResultItems = testResultItems
            .OrderBy(m => m.ProcessedObservation?.Taxon!.Attributes.RedlistCategory)
            .ToList();
        return testResultItems;
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByTaxonIds()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(o => o.TaxonId = TaxonIds.Skata)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Silvergran)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Talgoxe)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Blåmes)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = new int[] { TaxonIds.Silvergran, TaxonIds.Talgoxe, TaxonIds.Blåmes }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 out of 5 observations added to Elasticsearch had one of the Taxon Silvergran, Blåmes, Talgoxe.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsAsymmetricalResults_WhenComparingTaxonomies()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(1)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(o => o.TaxonId = TaxonIds.GråtrutxHavstrut)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var dyntaxaResponse = await apiClient.PostAsync($"/observations/search", JsonContent.Create(SearchFilterDtoFactory.CreateWithTaxonIds(true, TaxonIds.Gråtrut)), TestContext.Current.CancellationToken);
        var dyntaxaResult = await dyntaxaResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(cancellationToken: TestContext.Current.CancellationToken);

        var artportalenResponse = await apiClient.PostAsync($"/observations/search", JsonContent.Create(SearchFilterDtoFactory.CreateWithTaxonomyType(true, TaxonomyType.Artportalen, TaxonIds.Gråtrut)), TestContext.Current.CancellationToken);
        var artportalenResult = await artportalenResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        dyntaxaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        artportalenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        dyntaxaResult!.TotalCount.Should().Be(1, because: "the added observation belongs to an underlying taxon of Gråtrut.");
        artportalenResult!.TotalCount.Should().Be(0, because: "Artportalen taxonomy does not include the hybrid species added to Elasticsearch.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForUnderlyingTaxa()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(o => o.TaxonId = TaxonIds.Svartmes)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Blåmes)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Tofsmes)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Duvhök)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonIds(true, TaxonIds.Mesar);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 out of 5 observations added to Elasticsearch has Taxon Mesar as ancestor.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchByTaxonListMerge()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Asknätfjäril)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Sjögull)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jätteloka)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonListId(
            (int)TaxonListId.ProtectedByLaw, TaxonListOperatorDto.Merge, TaxonIds.Jättebalsamin);
        
        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "Svartfläckig blåvinge (1 record) and Asknätfjäril (1 record) are protected by law " +
                     "and Jättebalsamin (1 record) is merged with the taxa list.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchByTaxonListFilter()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge)
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithTaxonListId(
            (int)TaxonListId.ProtectedByLaw, TaxonListOperatorDto.Filter, TaxonIds.ViolettGuldvinge, TaxonIds.Jättebalsamin);

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "Violett guldvinge (3 records) is protected by law but not Jättebalsamin.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenTaxonListFilterResultsInNoTaxa()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin) // Not protected by law
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonListIds = new[] { (int)TaxonListId.ProtectedByLaw },
                Ids = new List<int> { TaxonIds.Jättebalsamin }, // Not protected by law
                TaxonListOperator = TaxonListOperatorDto.Filter
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "The taxon filter results in no taxa");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenTaxonListFilterWithoutTaxonIds()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge) // Protected by law
             .TheNext(1).With(o => o.TaxonId = TaxonIds.Jättebalsamin) // Not protected by law
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonListIds = new[] { (int)TaxonListId.ProtectedByLaw },
                TaxonListOperator = TaxonListOperatorDto.Filter
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(4,
            because: "4 observations are protected by law. No TaxonIds is given, which means all taxa filtered by protected by law taxa.");
    }


    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilterByTaxonCategories()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).HaveTaxonCategoryTaxonId(17)
             .TheNext(1).HaveTaxonCategoryTaxonId(17)
             .TheNext(1).HaveTaxonCategoryTaxonId(18)
             .TheNext(1).HaveTaxonCategoryTaxonId(14)
             .TheNext(1).HaveTaxonCategoryTaxonId(11)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonCategories = new int[] { 17, 18 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 record has taxon category 17 or 18.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenFilterByTaxonCategoriesThatNoObservationsHave()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(1).HaveTaxonCategoryTaxonId(17)
             .TheNext(1).HaveTaxonCategoryTaxonId(17)
             .TheNext(1).HaveTaxonCategoryTaxonId(18)
             .TheNext(1).HaveTaxonCategoryTaxonId(14)
             .TheNext(1).HaveTaxonCategoryTaxonId(11)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonCategories = new int[] { 1, 2 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "0 record has taxon category 1 or 2.");
    }
}
