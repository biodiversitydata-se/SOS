﻿using FizzWare.NBuilder;
using MongoDB.Driver.GeoJsonObjectModel;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class AdaptiveSearchLocationsTests : TestBase
{
    public AdaptiveSearchLocationsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReturnsLocations_AndUsesCachedRecordsOnSecondRequest()
    {
        // Arrange
        var observations = CreatePointObservations(
            6,
            310000,
            TaxonIds.Skata,
            index => (13.850 + index * 0.001, 58.380 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Skata, 13.84, 58.40, 13.90, 58.35);

        // Act
        var firstResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var firstResponseString = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var secondResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var secondResponseString = await secondResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var firstFeatures = JsonNode.Parse(firstResponseString)!["features"]!.AsArray();
        var secondFeatures = JsonNode.Parse(secondResponseString)!["features"]!.AsArray();

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        secondResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        firstResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        secondResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
        firstFeatures.Count.Should().Be(6);
        secondFeatures.Count.Should().Be(firstFeatures.Count);
        secondFeatures.Should().OnlyContain(feature => feature!["properties"]!["observations"] == null);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReusesCachedLargerBoundingBox_WhenRequestIsContained()
    {
        // Arrange
        var observations = CreatePointObservations(
            6,
            320000,
            TaxonIds.Silvergran,
            index => index < 3
                ? (13.851 + index * 0.001, 58.381 + index * 0.001)
                : (13.872 + index * 0.001, 58.388 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var largeFilter = CreateBoundingBoxFilter(TaxonIds.Silvergran, 13.84, 58.40, 13.90, 58.35);
        var containedFilter = CreateBoundingBoxFilter(TaxonIds.Silvergran, 13.849, 58.386, 13.855, 58.379);

        // Act
        var largeResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(largeFilter), TestContext.Current.CancellationToken);
        var containedResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(containedFilter), TestContext.Current.CancellationToken);
        var containedResponseString = await containedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var containedFeatures = JsonNode.Parse(containedResponseString)!["features"]!.AsArray();

        // Assert
        largeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        containedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        largeResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        containedResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
        containedResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        containedResponse.GetHeaderValue("X-Feature-Count").Should().Be("3");
        containedFeatures.Count.Should().Be(3);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReturnsGridAggregation_WithoutCaching_WhenLimitIsExceeded()
    {
        // Arrange
        var observations = CreatePointObservations(
            12,
            330000,
            TaxonIds.Talgoxe,
            index => (13.850 + index * 0.001, 58.380 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Talgoxe, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=5&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        response.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("12");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_IncludesFilterAreasInLocationResult_AlsoOnCacheHit()
    {
        // Arrange
        var observations = CreatePointObservations(
            1,
            330100,
            TaxonIds.Skata,
            _ => (13.850, 58.380));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxAndGeometryFilter(
            TaxonIds.Skata,
            bboxLeft: 13.84,
            bboxTop: 58.40,
            bboxRight: 13.90,
            bboxBottom: 58.35,
            geometryLeft: 13.84,
            geometryTop: 58.40,
            geometryRight: 13.90,
            geometryBottom: 58.35);
        const string url = "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&includeFilterAreasInResult=true";

        // Act
        var firstResponse = await apiClient.PostAsync(url, JsonContent.Create(searchFilter, options: JsonSerializerOptions), TestContext.Current.CancellationToken);
        var firstResponseString = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var secondResponse = await apiClient.PostAsync(url, JsonContent.Create(searchFilter, options: JsonSerializerOptions), TestContext.Current.CancellationToken);
        var secondResponseString = await secondResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        secondResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
        firstResponse.GetHeaderValue("X-Feature-Count").Should().Be("1");
        secondResponse.GetHeaderValue("X-Feature-Count").Should().Be("1");
        CountFeaturesWithFeatureType(firstResponseString, "FilterArea.OwnPolygon").Should().Be(1);
        CountFeaturesWithFeatureType(secondResponseString, "FilterArea.OwnPolygon").Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_IncludesFilterAreasInGridResult()
    {
        // Arrange
        var observations = CreatePointObservations(
            12,
            330200,
            TaxonIds.Talgoxe,
            index => (13.850 + index * 0.001, 58.380 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxAndGeometryFilter(
            TaxonIds.Talgoxe,
            bboxLeft: 13.84,
            bboxTop: 58.40,
            bboxRight: 13.90,
            bboxBottom: 58.35,
            geometryLeft: 13.84,
            geometryTop: 58.40,
            geometryRight: 13.90,
            geometryBottom: 58.35);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=5&cluster=false&returnInputBBox=true&includeFilterAreasInResult=true",
            JsonContent.Create(searchFilter, options: JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        CountFeaturesWithFeatureType(responseString, "FilterArea.OwnPolygon").Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReturnsEmptyLocations_WhenBoundingBoxAndGeometryDoNotOverlap()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxAndGeometryFilter(
            TaxonIds.Skata,
            bboxLeft: 15.090691863610152,
            bboxTop: 57.952641769832915,
            bboxRight: 15.33645792033352,
            bboxBottom: 57.86314388835064,
            geometryLeft: 14.59308654959814,
            geometryTop: 58.1518303241699,
            geometryRight: 15.081282562776002,
            geometryBottom: 57.92749551085515);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=0&adaptiveSearchStrategy=LocationsThenGrid&cluster=false",
            JsonContent.Create(searchFilter, options: JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var features = JsonNode.Parse(responseString)!["features"]!.AsArray();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("0");
        response.GetHeaderValue("X-Estimated-Location-Count").Should().Be("0");
        response.GetHeaderValue("X-Feature-Count").Should().Be("0");
        response.GetHeaderValue("X-Locations-Before-Clustering").Should().Be("0");
        response.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        features.Count.Should().Be(0);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_UsesObservationsThenGridByDefault_AndIgnoresLocationAggregationLimit()
    {
        // Arrange
        var observations = CreatePresentLocationObservations(
            4,
            331101,
            TaxonIds.Duvhök,
            "Default strategy location",
            14.010,
            58.110);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Duvhök, 14.00, 58.12, 14.02, 58.10);

        // Act
        var defaultStrategyResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=1&cluster=false&returnInputBBox=true&locationAggregationLimit=2", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var explicitStrategyResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=1&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=ObservationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        defaultStrategyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        explicitStrategyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        defaultStrategyResponse.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        explicitStrategyResponse.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        defaultStrategyResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
        explicitStrategyResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ObservationsThenLocationsThenGrid_PrioritizesFullObservations_AndSkipsLocationsWhenLimitIsMissing()
    {
        // Arrange
        const int siteId = 331201;
        var observations = CreatePresentLocationObservations(
            4,
            siteId,
            TaxonIds.ViolettGuldvinge,
            "Strategy two full location",
            14.030,
            58.130);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.ViolettGuldvinge, 14.02, 58.14, 14.04, 58.12);

        // Act
        var fullLocationsResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var fullLocationsResponseString = await fullLocationsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(fullLocationsResponseString);
        var properties = propertiesByLocationId[CreateArtportalenSiteLocationId(siteId)];
        var noLocationLimitResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=1&cluster=false&returnInputBBox=true&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        fullLocationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        fullLocationsResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        fullLocationsResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        propertiesByLocationId.Should().ContainSingle();        
        properties["observationCount"]!.GetValue<int>().Should().Be(4);
        GetPresentation(properties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(4);
        GetObservationSummary(properties)["statusCounts"]!["foundCount"]!.GetValue<int>().Should().Be(4);

        noLocationLimitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        noLocationLimitResponse.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        noLocationLimitResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ObservationsThenLocationsThenGrid_ReturnsEstimatedLocationAggregation_WithSingleRepresentative()
    {
        // Arrange
        const int siteId = 331001;
        var observations = CreatePresentLocationObservations(
            4,
            siteId,
            TaxonIds.Duvhök,
            "Single representative location",
            13.855,
            58.383);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Duvhök, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=1&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);
        var properties = propertiesByLocationId[CreateArtportalenSiteLocationId(siteId)];

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        response.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        response.GetHeaderValue("X-Location-Aggregation-Mode").Should().Be("representative");
        response.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("true");
        response.Headers.Contains("X-Location-Aggregation-Representatives-Count").Should().BeFalse();
        propertiesByLocationId.Should().ContainSingle();        
        properties["isSummaryApproximated"]!.GetValue<bool>().Should().BeTrue();
        properties["observationCount"]!.GetValue<int>().Should().Be(4);
        GetPresentation(properties)["status"]!.GetValue<string>().Should().Be("Present");
        GetPresentation(properties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(1);
        GetObservationSummary(properties)["statusCounts"]!["foundCount"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ObservationsThenLocationsThenGrid_DoesNotMarkSingleObservationRepresentativeAsSummaryApproximated()
    {
        // Arrange
        const int siteId = 331003;
        var observations = CreatePresentLocationObservations(
            1,
            siteId,
            TaxonIds.Duvhök,
            "Single observation representative location",
            13.856,
            58.384);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Duvhök, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=0&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);
        var properties = propertiesByLocationId[CreateArtportalenSiteLocationId(siteId)];

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        response.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
        propertiesByLocationId.Should().ContainSingle();        
        properties["observationCount"]!.GetValue<int>().Should().Be(1);
        GetPresentation(properties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ObservationsThenLocationsThenGrid_ReturnsGridAggregation_WhenEstimatedLocationCountEqualsLocationAggregationLimit()
    {
        // Arrange
        var observations = CreatePresentLocationObservations(
                2,
                334001,
                TaxonIds.ViolettGuldvinge,
                "First strict threshold location",
                13.858,
                58.386)
            .Concat(CreatePresentLocationObservations(
                2,
                334002,
                TaxonIds.ViolettGuldvinge,
                "Second strict threshold location",
                13.868,
                58.387))
            .ToList();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.ViolettGuldvinge, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=1&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        response.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        response.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_LocationsThenGrid_ReturnsEstimatedLocationAggregation_EvenWhenObservationCountIsBelowLimit()
    {
        // Arrange
        const int siteId = 331301;
        var observations = CreatePresentLocationObservations(
            4,
            siteId,
            TaxonIds.Asknätfjäril,
            "Strategy three representative location",
            14.050,
            58.150);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Asknätfjäril, 14.04, 58.16, 14.06, 58.14);

        // Act
        var response = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=LocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);
        var properties = propertiesByLocationId[CreateArtportalenSiteLocationId(siteId)];

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        response.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        response.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("true");
        propertiesByLocationId.Should().ContainSingle();        
        properties["isSummaryApproximated"]!.GetValue<bool>().Should().BeTrue();
        properties["observationCount"]!.GetValue<int>().Should().Be(4);
        GetPresentation(properties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_LocationsThenGrid_BypassesFullLocationCache()
    {
        // Arrange
        const int siteId = 331501;
        var observations = CreatePresentLocationObservations(
            4,
            siteId,
            TaxonIds.Svartmes,
            "Strategy three cache bypass location",
            14.100,
            58.190);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Svartmes, 14.09, 58.20, 14.11, 58.18);

        // Act
        var cacheWarmupResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var cacheWarmupResponseString = await cacheWarmupResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var cachedProperties = GetFeaturePropertiesByLocationId(cacheWarmupResponseString)[CreateArtportalenSiteLocationId(siteId)];
        var strategyThreeResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=LocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var strategyThreeResponseString = await strategyThreeResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var strategyThreeProperties = GetFeaturePropertiesByLocationId(strategyThreeResponseString)[CreateArtportalenSiteLocationId(siteId)];

        // Assert
        cacheWarmupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cacheWarmupResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        cacheWarmupResponse.GetHeaderValue("X-Cache").Should().Be("MISS");        
        GetPresentation(cachedProperties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(4);

        strategyThreeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        strategyThreeResponse.GetHeaderValue("X-Result-Type").Should().Be("locations");
        strategyThreeResponse.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        strategyThreeResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("true");
        strategyThreeProperties["isSummaryApproximated"]!.GetValue<bool>().Should().BeTrue();
        strategyThreeProperties["observationCount"]!.GetValue<int>().Should().Be(4);
        GetPresentation(strategyThreeProperties)["statusCounts"]!["Present"]!.GetValue<int>().Should().Be(1);
    }   

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_SumsRepresentativeObservationCounts_WhenLocationIdsShareDistinctLocationId()
    {
        // Arrange
        const int firstSiteId = 331604;
        const int secondSiteId = 331605;
        var observations = CreatePresentLocationObservations(
                2,
                firstSiteId,
                TaxonIds.Svartmes,
                "First source location at same place",
                14.123,
                58.213)
            .Concat(CreatePresentLocationObservations(
                3,
                secondSiteId,
                TaxonIds.Svartmes,
                "Second source location at same place",
                14.123,
                58.213))
            .ToList();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Svartmes, 14.11, 58.22, 14.13, 58.20);

        // Act
        var response = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=0&cluster=false&returnInputBBox=true&locationAggregationLimit=10&adaptiveSearchStrategy=ObservationsThenLocationsThenGrid&useCache=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);
        var properties = propertiesByLocationId.Values.Single();
        var locationIds = properties["locations"]!["ids"]!.AsArray()
            .Select(locationId => locationId!.GetValue<string>())
            .ToList();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        response.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("true");
        propertiesByLocationId.Should().ContainSingle();
        properties["isSummaryApproximated"]!.GetValue<bool>().Should().BeTrue();
        properties["observationCount"]!.GetValue<int>().Should().Be(5);
        locationIds.Should().BeEquivalentTo(
            new[]
            {
                CreateArtportalenSiteLocationId(firstSiteId),
                CreateArtportalenSiteLocationId(secondSiteId)
            });
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_LocationsThenGrid_ReturnsGrid_WhenLocationAggregationLimitIsMissingOrNotMatched()
    {
        // Arrange
        var observations = CreatePresentLocationObservations(
                1,
                331401,
                TaxonIds.SvartfläckigBlåvinge,
                "Strategy three first grid location",
                14.070,
                58.170)
            .Concat(CreatePresentLocationObservations(
                1,
                331402,
                TaxonIds.SvartfläckigBlåvinge,
                "Strategy three second grid location",
                14.080,
                58.171))
            .ToList();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.SvartfläckigBlåvinge, 14.06, 58.18, 14.09, 58.16);

        // Act
        var missingLocationLimitResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&adaptiveSearchStrategy=LocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var equalLocationLimitResponse = await apiClient.PostAsync("/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&locationAggregationLimit=2&adaptiveSearchStrategy=LocationsThenGrid", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        missingLocationLimitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        equalLocationLimitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        missingLocationLimitResponse.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        equalLocationLimitResponse.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        missingLocationLimitResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
        equalLocationLimitResponse.GetHeaderValue("X-Location-Summary-Approximated").Should().Be("false");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_SkipsCountAndExtent_WhenContainedBoundingBoxCanBeServedFromCache()
    {
        // Arrange
        var observations = CreatePointObservations(
            6,
            340000,
            TaxonIds.Blåmes,
            index => index < 3
                ? (13.851 + index * 0.001, 58.381 + index * 0.001)
                : (13.872 + index * 0.001, 58.388 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var observationManagerProbe = new ObservationManagerCallProbe();
        var apiClient = CreateApiClientWithObservationManagerProbe(observationManagerProbe);
        var largeFilter = CreateBoundingBoxFilter(TaxonIds.Blåmes, 13.84, 58.40, 13.90, 58.35);
        var containedFilter = CreateBoundingBoxFilter(TaxonIds.Blåmes, 13.849, 58.386, 13.855, 58.379);

        // Act
        var largeResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(largeFilter), TestContext.Current.CancellationToken);
        var countAndExtentCallsAfterLargeRequest = observationManagerProbe.CountAndExtentCalls;
        var containedResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(containedFilter), TestContext.Current.CancellationToken);

        // Assert
        largeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        containedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        largeResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        containedResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
        countAndExtentCallsAfterLargeRequest.Should().Be(1);
        observationManagerProbe.CountAndExtentCalls.Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_QueriesAgainForAdjacentBoundingBoxes_EvenWhenRequestIsCovered()
    {
        // Arrange
        var observations = CreatePointObservations(
            4,
            346000,
            TaxonIds.Mesar,
            index => index switch
            {
                0 => (13.851, 58.381),
                1 => (13.855, 58.392),
                2 => (13.865, 58.383),
                _ => (13.875, 58.390)
            });
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var observationManagerProbe = new ObservationManagerCallProbe();
        var apiClient = CreateApiClientWithObservationManagerProbe(observationManagerProbe);
        var leftFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 13.84, 58.40, 13.86, 58.35);
        var rightFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 13.86, 58.40, 13.88, 58.35);
        var combinedFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 13.84, 58.40, 13.88, 58.35);

        // Act
        var leftResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(leftFilter), TestContext.Current.CancellationToken);
        var rightResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(rightFilter), TestContext.Current.CancellationToken);
        var countAndExtentCallsAfterWarmupRequests = observationManagerProbe.CountAndExtentCalls;
        var combinedResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(combinedFilter), TestContext.Current.CancellationToken);
        var combinedResponseString = await combinedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var combinedFeatures = JsonNode.Parse(combinedResponseString)!["features"]!.AsArray();

        // Assert
        leftResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        rightResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        combinedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        leftResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        rightResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        combinedResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        combinedResponse.GetHeaderValue("X-Feature-Count").Should().Be("4");
        combinedFeatures.Count.Should().Be(4);
        countAndExtentCallsAfterWarmupRequests.Should().Be(2);
        observationManagerProbe.CountAndExtentCalls.Should().Be(3);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_DoesNotReuseCachedBoundingBoxes_WhenCoverageHasGap()
    {
        // Arrange
        var observations = CreatePointObservations(
            3,
            346100,
            TaxonIds.Mesar,
            index => index switch
            {
                0 => (14.201, 58.481),
                1 => (14.220, 58.482),
                _ => (14.239, 58.483)
            });
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var observationManagerProbe = new ObservationManagerCallProbe();
        var apiClient = CreateApiClientWithObservationManagerProbe(observationManagerProbe);
        var leftFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 14.19, 58.50, 14.21, 58.45);
        var rightFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 14.23, 58.50, 14.25, 58.45);
        var combinedFilter = CreateBoundingBoxFilter(TaxonIds.Mesar, 14.19, 58.50, 14.25, 58.45);

        // Act
        var leftResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(leftFilter), TestContext.Current.CancellationToken);
        var rightResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(rightFilter), TestContext.Current.CancellationToken);
        var countAndExtentCallsAfterWarmupRequests = observationManagerProbe.CountAndExtentCalls;
        var combinedResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(combinedFilter), TestContext.Current.CancellationToken);
        var combinedResponseString = await combinedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var combinedPropertiesByLocationId = GetFeaturePropertiesByLocationId(combinedResponseString);

        // Assert
        leftResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        rightResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        combinedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        leftResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        rightResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        combinedResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        combinedResponse.GetHeaderValue("X-Feature-Count").Should().Be("3");
        combinedPropertiesByLocationId.Should().ContainKey(CreateArtportalenSiteLocationId(346101));
        countAndExtentCallsAfterWarmupRequests.Should().Be(2);
        observationManagerProbe.CountAndExtentCalls.Should().Be(3);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_BypassesLocationCache_WhenUseCacheIsFalse()
    {
        // Arrange
        var observations = CreatePointObservations(
            6,
            345000,
            TaxonIds.Tofsmes,
            index => (13.851 + index * 0.001, 58.381 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Tofsmes, 13.84, 58.40, 13.90, 58.35);

        // Act
        var bypassResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&useCache=false",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var cacheEnabledResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var cachedResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);

        // Assert
        bypassResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cacheEnabledResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cachedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        bypassResponse.GetHeaderValue("X-Cache").Should().Be("BYPASS");
        cacheEnabledResponse.GetHeaderValue("X-Cache").Should().Be("MISS", "the bypassed request must not write a cache entry");
        cachedResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_UsesMinimumLocationCountBeforeClusteringThreshold()
    {
        // Arrange
        var observations = CreatePointObservations(
            3,
            380000,
            TaxonIds.Sjögull,
            index => (13.8500 + index * 0.0001, 58.3800 + index * 0.0001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 13.84, 58.40, 13.90, 58.35);
        const string baseUrl = "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=true&returnInputBBox=true&clusteringAlgorithm=Sweref99TmFastGreedy&metersPerPixel=1000&clusterRadiusPx=100&minLocationsPerCluster=2";

        // Act
        var thresholdEqualsCountResponse = await apiClient.PostAsync(
            $"{baseUrl}&minLocationCountBeforeClustering=3",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var thresholdEqualsCountResponseString = await thresholdEqualsCountResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var thresholdBelowCountResponse = await apiClient.PostAsync(
            $"{baseUrl}&minLocationCountBeforeClustering=2",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var thresholdBelowCountResponseString = await thresholdBelowCountResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var thresholdEqualsCountFeatures = JsonNode.Parse(thresholdEqualsCountResponseString)!["features"]!.AsArray();
        var thresholdBelowCountFeatures = JsonNode.Parse(thresholdBelowCountResponseString)!["features"]!.AsArray();

        // Assert
        thresholdEqualsCountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        thresholdBelowCountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        thresholdEqualsCountFeatures.Count.Should().Be(3, "clustering starts only when distinct location count is greater than the threshold");
        thresholdBelowCountFeatures.Count.Should().Be(1);
        thresholdBelowCountFeatures[0]!["properties"]!["isCluster"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReturnsBadRequest_WhenPolygonWhenNeededLacksMetersPerPixel()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&createPolygons=WhenNeeded",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseString.Should().Contain("metersPerPixel");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ClustersWithBoundingBoxFallback_WhenMetersPerPixelIsMissing()
    {
        // Arrange
        var observations = CreatePointObservations(
            3,
            390000,
            TaxonIds.Sjögull,
            index => (13.8500 + index * 0.0001, 58.3800 + index * 0.0001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 13.84, 58.40, 13.90, 58.35);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=true&returnInputBBox=true&clusteringAlgorithm=Sweref99TmFastGreedy&minLocationsPerCluster=2&minLocationCountBeforeClustering=2",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var features = JsonNode.Parse(responseString)!["features"]!.AsArray();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        features.Should().ContainSingle();
        features[0]!["properties"]!["isCluster"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_DocumentsPresentationStatusesWithoutCutoffYear()
    {
        // Arrange
        const int presentSiteId = 350001;
        const int notRecoveredSiteId = 350002;
        const int notPresentSiteId = 350003;
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All()
                .HaveValuesFromPredefinedObservations()
            .Build();
        ConfigureLocationObservation(
            observations[0],
            presentSiteId,
            TaxonIds.Jättebalsamin,
            "Present location",
            13.850,
            58.380,
            notPresent: false,
            notRecovered: false,
            endYear: 2025);
        ConfigureLocationObservation(
            observations[1],
            notRecoveredSiteId,
            TaxonIds.Jättebalsamin,
            "Not recovered location",
            13.860,
            58.385,
            notPresent: false,
            notRecovered: true,
            endYear: 2025);
        ConfigureLocationObservation(
            observations[2],
            notPresentSiteId,
            TaxonIds.Jättebalsamin,
            "Never found location",
            13.870,
            58.390,
            notPresent: true,
            notRecovered: false,
            endYear: 2025);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateLocationPresentationStatusDocumentationFilter(TaxonIds.Jättebalsamin, 2025, 2025);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        propertiesByLocationId.Count.Should().Be(3);
        AssertLocationFeature(propertiesByLocationId, presentSiteId, "Present", expectedCutoffYearStatus: null, expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, notRecoveredSiteId, "NotRecovered", expectedCutoffYearStatus: null, expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, notPresentSiteId, "NotPresent", expectedCutoffYearStatus: null, expectedIsAllObservationsSensitive: false);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_DocumentsPresentationStatusesAndCutoffYearStatusWithCutoffYear()
    {
        // Arrange
        const int presentAfterSiteId = 360001;
        const int presentUntilSiteId = 360002;
        const int notRecoveredAfterSiteId = 360003;
        const int notRecoveredUntilSiteId = 360004;
        const int notPresentSiteId = 360005;
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All()
                .HaveValuesFromPredefinedObservations()
            .Build();
        ConfigureLocationObservation(
            observations[0],
            presentAfterSiteId,
            TaxonIds.Jätteloka,
            "Present after cutoff",
            13.850,
            58.380,
            notPresent: false,
            notRecovered: false,
            endYear: 2025);
        ConfigureLocationObservation(
            observations[1],
            presentUntilSiteId,
            TaxonIds.Jätteloka,
            "Present until cutoff",
            13.860,
            58.385,
            notPresent: false,
            notRecovered: false,
            endYear: 2020);
        ConfigureLocationObservation(
            observations[2],
            notRecoveredAfterSiteId,
            TaxonIds.Jätteloka,
            "Not recovered after cutoff",
            13.870,
            58.390,
            notPresent: false,
            notRecovered: true,
            endYear: 2025);
        ConfigureLocationObservation(
            observations[3],
            notRecoveredUntilSiteId,
            TaxonIds.Jätteloka,
            "Not recovered until cutoff",
            13.880,
            58.395,
            notPresent: false,
            notRecovered: true,
            endYear: 2020);
        ConfigureLocationObservation(
            observations[4],
            notPresentSiteId,
            TaxonIds.Jätteloka,
            "Never found location",
            13.890,
            58.399,
            notPresent: true,
            notRecovered: false,
            endYear: 2025);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateLocationPresentationStatusDocumentationFilter(TaxonIds.Jätteloka, 2020, 2025);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&cutoffYear=2024",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        propertiesByLocationId.Count.Should().Be(5);
        AssertLocationFeature(propertiesByLocationId, presentAfterSiteId, "Present", "After", expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, presentUntilSiteId, "DividedByYearUntil", "Until", expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, notRecoveredAfterSiteId, "NotPresent", "AfterNotRecovered", expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, notRecoveredUntilSiteId, "NotPresent", "UntilNotRecovered", expectedIsAllObservationsSensitive: false);
        AssertLocationFeature(propertiesByLocationId, notPresentSiteId, "NotPresent", "NotPresent", expectedIsAllObservationsSensitive: false);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_ReusesCachedLocationAggregates_WithCutoffYear()
    {
        // Arrange
        const int siteId = 365001;
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(1)
            .All()
                .HaveValuesFromPredefinedObservations()
            .Build();
        ConfigureLocationObservation(
            observations[0],
            siteId,
            TaxonIds.Jättebalsamin,
            "Present before cutoff",
            13.850,
            58.380,
            notPresent: false,
            notRecovered: false,
            endYear: 2020);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = CreateLocationPresentationStatusDocumentationFilter(TaxonIds.Jättebalsamin, 2020, 2020);

        // Act
        var cacheWarmupResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var cutoffResponse = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&cutoffYear=2024",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var cutoffResponseString = await cutoffResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(cutoffResponseString);

        // Assert
        cacheWarmupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cutoffResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        cacheWarmupResponse.GetHeaderValue("X-Cache").Should().Be("MISS");
        cutoffResponse.GetHeaderValue("X-Cache").Should().Be("HIT");
        AssertLocationFeature(propertiesByLocationId, siteId, "DividedByYearUntil", "Until", expectedIsAllObservationsSensitive: false);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternal_DocumentsSensitiveLocationFlags()
    {
        // Arrange
        const int sensitiveSiteId = 370001;
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All()
                .HaveValuesFromPredefinedObservations()
                .HaveTaxonSensitivityCategory(3)
            .Build();
        var sensitiveTaxonId = observations[0].TaxonId!.Value;
        foreach (var observation in observations)
        {
            ConfigureLocationObservation(
                observation,
                sensitiveSiteId,
                sensitiveTaxonId,
                "Sensitive present location",
                13.855,
                58.383,
                notPresent: false,
                notRecovered: false,
                endYear: 2025);
            observation.ProtectedBySystem = true;
        }

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = CreateLocationPresentationStatusDocumentationFilter(sensitiveTaxonId, 2025, 2025);

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&returnInputBBox=true&sensitiveObservations=true&cutoffYear=2024",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var propertiesByLocationId = GetFeaturePropertiesByLocationId(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("locations");
        propertiesByLocationId.Count.Should().Be(1);
        AssertLocationFeature(propertiesByLocationId, sensitiveSiteId, "PresentSensitive", "After", expectedIsAllObservationsSensitive: true);
        propertiesByLocationId[CreateArtportalenSiteLocationId(sensitiveSiteId)]["observationCount"].GetValue<int>().Should().Be(2);
    }

    private static IList<ArtportalenObservationVerbatim> CreatePointObservations(
        int count,
        int siteIdStart,
        int taxonId,
        Func<int, (double Longitude, double Latitude)> coordinateFactory)
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(count)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = taxonId)
            .Build();

        for (var index = 0; index < observations.Count; index++)
        {
            var coordinate = coordinateFactory(index);
            observations[index].Site.HasGeometry = false;
            observations[index].Site.Id = siteIdStart + index;
            observations[index].Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(
                new GeoJson2DCoordinates(coordinate.Longitude, coordinate.Latitude));
        }

        return observations;
    }

    private static IList<ArtportalenObservationVerbatim> CreatePresentLocationObservations(
        int count,
        int siteId,
        int taxonId,
        string siteName,
        double longitude,
        double latitude)
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(count)
            .All()
                .HaveValuesFromPredefinedObservations()
            .Build();

        foreach (var observation in observations)
        {
            ConfigureLocationObservation(
                observation,
                siteId,
                taxonId,
                siteName,
                longitude,
                latitude,
                notPresent: false,
                notRecovered: false,
                endYear: 2025);
        }

        return observations;
    }

    private static void ConfigureLocationObservation(
        ArtportalenObservationVerbatim observation,
        int siteId,
        int taxonId,
        string siteName,
        double longitude,
        double latitude,
        bool notPresent,
        bool notRecovered,
        int endYear)
    {
        var observationDate = DateTime.SpecifyKind(new DateTime(endYear, 6, 1), DateTimeKind.Local);
        observation.TaxonId = taxonId;
        observation.NotPresent = notPresent;
        observation.NotRecovered = notRecovered;
        observation.StartDate = observationDate;
        observation.EndDate = observationDate;
        observation.StartTime = TimeSpan.Zero;
        observation.EndTime = TimeSpan.Zero;
        observation.Site.HasGeometry = false;
        observation.Site.Id = siteId;
        observation.Site.Name = siteName;
        observation.Site.Accuracy = 25;
        observation.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(
            new GeoJson2DCoordinates(longitude, latitude));
    }

    private static SearchFilterInternalDto CreateLocationPresentationStatusDocumentationFilter(int taxonId, int startYear, int endYear)
    {
        var searchFilter = CreateBoundingBoxFilter(taxonId, 13.84, 58.41, 13.91, 58.35);
        searchFilter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent;
        searchFilter.NotRecoveredFilter = SightingNotRecoveredFilterDto.NoFilter;
        searchFilter.ExtendedFilter = new ExtendedFilterDto
        {
            NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
        };
        searchFilter.Date = new DateFilterDto
        {
            StartDate = DateTime.SpecifyKind(new DateTime(startYear, 1, 1), DateTimeKind.Local),
            EndDate = DateTime.SpecifyKind(new DateTime(endYear, 12, 31), DateTimeKind.Local),
            DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
        };

        return searchFilter;
    }

    private static Dictionary<string, JsonObject> GetFeaturePropertiesByLocationId(string responseString)
    {
        return JsonNode.Parse(responseString)!["features"]!.AsArray()
            .Select(feature => feature!["properties"]!.AsObject())
            .ToDictionary(
                properties => properties["locations"]!["location"]!["locationId"]!.GetValue<string>(),
                properties => properties,
                StringComparer.Ordinal);
    }

    private static JsonObject GetPresentation(JsonObject properties)
    {
        return properties["presentation"]!.AsObject();
    }

    private static JsonObject GetObservationSummary(JsonObject properties)
    {
        return properties["observationSummary"]!.AsObject();
    }

    private static int CountFeaturesWithFeatureType(string responseString, string featureType)
    {
        return JsonNode.Parse(responseString)!["features"]!.AsArray()
            .Count(feature => feature?["properties"]?["FeatureType"]?.GetValue<string>() == featureType);
    }

    private static void AssertLocationFeature(
        IReadOnlyDictionary<string, JsonObject> propertiesByLocationId,
        int siteId,
        string expectedPresentationStatus,
        string? expectedCutoffYearStatus,
        bool expectedIsAllObservationsSensitive)
    {
        var locationId = CreateArtportalenSiteLocationId(siteId);
        propertiesByLocationId.Should().ContainKey(locationId);
        var properties = propertiesByLocationId[locationId];
        var presentation = GetPresentation(properties);

        presentation["status"]!.GetValue<string>().Should().Be(expectedPresentationStatus);
        presentation["cutoffYearStatus"]?.GetValue<string>().Should().Be(expectedCutoffYearStatus);
        presentation["isAllObservationsSensitive"]!.GetValue<bool>().Should().Be(expectedIsAllObservationsSensitive);
    }

    private static string CreateArtportalenSiteLocationId(int siteId)
    {
        return $"urn:lsid:artportalen.se:site:{siteId}";
    }

    private HttpClient CreateApiClientWithObservationManagerProbe(ObservationManagerCallProbe probe)
    {
        return TestFixture.ApiFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped<IObservationManager>(serviceProvider =>
                {
                    var inner = ActivatorUtilities.CreateInstance<ObservationManager>(serviceProvider);
                    var mock = new Mock<IObservationManager>(MockBehavior.Strict);
                    mock.SetupGet(manager => manager.MaxNrElasticSearchAggregationBuckets)
                        .Returns(inner.MaxNrElasticSearchAggregationBuckets);
                    mock.Setup(manager => manager.PrepareObservationFilterAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<SearchFilter>()))
                        .Returns((int? roleId, string application, SearchFilter filter) => inner.PrepareObservationFilterAsync(roleId, application, filter));
                    mock.Setup(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<bool>()))
                        .Returns((PreparedObservationFilter filter, bool includeExtent) =>
                        {
                            probe.CountAndExtentCalls++;
                            return inner.GetCountAndExtentAsync(filter, includeExtent);
                        });
                    mock.Setup(manager => manager.GetChunkAsync(
                            It.IsAny<PreparedObservationFilter>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<long?>()))
                        .Returns((PreparedObservationFilter filter, int skip, int take, long? knownTotalCount) =>
                        {
                            probe.ChunkCalls++;
                            return inner.GetChunkAsync(filter, skip, take, knownTotalCount);
                        });
                    mock.Setup(manager => manager.GetGeogridTileAggregationAsync(
                            It.IsAny<PreparedObservationFilter>(),
                            It.IsAny<int>()))
                        .Returns((PreparedObservationFilter filter, int precision) => inner.GetGeogridTileAggregationAsync(filter, precision));

                    return mock.Object;
                }));
            });
        }).CreateClient();
    }

    private static SearchFilterInternalDto CreateBoundingBoxFilter(
        int taxonId,
        double left,
        double top,
        double right,
        double bottom)
    {
        return new SearchFilterInternalDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = [taxonId]
            },
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = new LatLonBoundingBoxDto
                {
                    TopLeft = new LatLonCoordinateDto
                    {
                        Longitude = left,
                        Latitude = top
                    },
                    BottomRight = new LatLonCoordinateDto
                    {
                        Longitude = right,
                        Latitude = bottom
                    }
                }
            }
        };
    }

    private static SearchFilterInternalDto CreateBoundingBoxAndGeometryFilter(
        int taxonId,
        double bboxLeft,
        double bboxTop,
        double bboxRight,
        double bboxBottom,
        double geometryLeft,
        double geometryTop,
        double geometryRight,
        double geometryBottom)
    {
        var searchFilter = CreateBoundingBoxFilter(taxonId, bboxLeft, bboxTop, bboxRight, bboxBottom);
        searchFilter.Geographics!.Geometries = new[]
        {
            new Polygon(new LinearRing(new[]
            {
                new Coordinate(geometryLeft, geometryBottom),
                new Coordinate(geometryRight, geometryBottom),
                new Coordinate(geometryRight, geometryTop),
                new Coordinate(geometryLeft, geometryTop),
                new Coordinate(geometryLeft, geometryBottom)
            }))
        };

        return searchFilter;
    }

    private sealed class ObservationManagerCallProbe
    {
        public int CountAndExtentCalls { get; set; }

        public int ChunkCalls { get; set; }
    }
}
