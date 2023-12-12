using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
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
        var searchFilter = new SearchFilterDto {
            Taxon = new TaxonFilterDto {
                RedListCategories = new string[] { "CR", "EN", "VU" }
            }
        };        

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
        var searchFilter = new SearchFilterDto {
            Taxon = new TaxonFilterDto {
                Ids = new int[] { TaxonIds.Silvergran, TaxonIds.Talgoxe, TaxonIds.Blåmes }
            }
        };        

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
        var searchFilter = new SearchFilterDto {
            Taxon = new TaxonFilterDto {
                Ids = new int[] { TaxonIds.Mesar },
                IncludeUnderlyingTaxa = true
            }
        };        

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
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenTaxonListFilterResultsInNoTaxa()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin) // Not protected by law
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "The taxon filter results in no taxa");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenTaxonListFilterWithoutTaxonIds()
    {
        // Arrange        
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.ViolettGuldvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.SvartfläckigBlåvinge) // Protected by law
             .TheNext(20).With(o => o.TaxonId = TaxonIds.Jättebalsamin) // Not protected by law
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(80,
            because: "80 observations are protected by law. No TaxonIds is given, which means all taxa filtered by protected by law taxa.");
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
        var searchFilter = new SearchFilterDto {
            Taxon = new TaxonFilterDto {
                TaxonCategories = new int[] { 17, 18 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 record has taxon category 17 or 18.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenFilterByTaxonCategoriesThatNoObservationsHave()
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
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonCategories = new int[] { 1, 2 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "0 record has taxon category 1 or 2.");
    }
}