using SOS.Api.Contracts.Areas.Responses;
using FizzWare.NBuilder;
using GeoJSON.Text.Geometry;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.TestHelpers.Gis;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.SignalSearchInternalEndpoint;

[Collection(TestCollection.Name)]
public class SignalSearchTests : ElasticsearchTestBase
{
    public SignalSearchTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }
   
    [Fact]
    public async Task SignalSearchInternal_ReturnsTrue_WhenUserHasPermission()
    {
        // Arrange
        int sensitiveTaxonId = ProtectedSpeciesHelper.SensitiveSpeciesByCategory[5].First();
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All()
                .HaveValuesFromPredefinedObservations(true)            
                .With(p => p.TaxonId = sensitiveTaxonId)
                .With(p => p.ProtectedBySystem = true)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .With(p => p.Site.County = new GeographicalArea { FeatureId = CountyId.Uppsala })
                .With(p => p.Site.Municipality = new GeographicalArea { FeatureId = MunicipalityId.Uppsala.ToString() })
                .HaveCoordinates(TestCoordinates.UppsalaMunicipality.Longitude, TestCoordinates.UppsalaMunicipality.Latitude, 50)            
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);        
        var userServiceStub = UserServiceStubFactory.CreateWithCountySightingIndicationAuthority(
            maxProtectionLevel: 1,
            countyFeatureId: CountyId.Uppsala);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var searchFilter = new SignalFilterDto
        {
            StartDate = new DateTime(1999, 12, 1),
            Geographics = new GeographicsFilterDto
            {

                BoundingBox = new LatLonBoundingBoxDto
                {
                    BottomRight = new LatLonCoordinateDto { Latitude = 50, Longitude = 25 },
                    TopLeft = new LatLonCoordinateDto { Latitude = 70, Longitude = 9 },
                }
            },
            Taxon = new TaxonFilterBaseDto
            {
                TaxonListIds = new[] { 1 },
                Ids = [sensitiveTaxonId]
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SignalSearchInternal_ReturnsFalse_WhenUserHasNoPermission()
    {
        // Arrange
        int sensitiveTaxonId = ProtectedSpeciesHelper.SensitiveSpeciesByCategory[5].First();
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All()
                .HaveValuesFromPredefinedObservations(true)
                .With(p => p.TaxonId = sensitiveTaxonId)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .With(p => p.Site.County = new GeographicalArea { FeatureId = CountyId.Uppsala })
                .With(p => p.Site.Municipality = new GeographicalArea { FeatureId = MunicipalityId.Uppsala.ToString() })
                .HaveCoordinates(TestCoordinates.UppsalaMunicipality.Longitude, TestCoordinates.UppsalaMunicipality.Latitude, 50)
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);        
        var userServiceStub = UserServiceStubFactory.CreateWithCountySightingIndicationAuthority(
            maxProtectionLevel: 1,
            countyFeatureId: CountyId.Jönköping);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var searchFilter = new SignalFilterDto
        {
            StartDate = new DateTime(1999, 12, 1),
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = new LatLonBoundingBoxDto
                {
                    BottomRight = new LatLonCoordinateDto { Latitude = 50, Longitude = 25 },
                    TopLeft = new LatLonCoordinateDto { Latitude = 70, Longitude = 9 },
                }
            },
            Taxon = new TaxonFilterBaseDto
            {
                TaxonListIds = new[] { 1 },
                Ids = [sensitiveTaxonId]
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SignalSearchInternal_ReturnsHttp403_WhenUserHasNoPermissions()
    {
        int sensitiveTaxonId = ProtectedSpeciesHelper.SensitiveSpeciesByCategory[5].First();
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All()
                .HaveValuesFromPredefinedObservations(true)
            .TheFirst(1)
                .With(p => p.TaxonId = sensitiveTaxonId)
                .With(p => p.ProtectedBySystem = true)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .With(p => p.Site.County = new GeographicalArea { FeatureId = CountyId.Uppsala })
                .With(p => p.Site.Municipality = new GeographicalArea { FeatureId = MunicipalityId.Uppsala.ToString() })
                .HaveCoordinates(TestCoordinates.UppsalaMunicipality.Longitude, TestCoordinates.UppsalaMunicipality.Latitude, 50)                
            .TheNext(1)
                .With(p => p.TaxonId = sensitiveTaxonId)
                .With(p => p.ProtectedBySystem = true)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .With(p => p.Site.County = new GeographicalArea { FeatureId = CountyId.Jönköping })
                .With(p => p.Site.Municipality = new GeographicalArea { FeatureId = MunicipalityId.Jönköping.ToString() })
                .HaveCoordinates(TestCoordinates.JönköpingMunicipality.Longitude, TestCoordinates.JönköpingMunicipality.Latitude, 50)                
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);       

        // Test 1 - User has permission for Uppsala county, search in Uppsala using bbox => Should return true
        var userServiceStub = UserServiceStubFactory.CreateWithCountySightingIndicationAuthority(
            maxProtectionLevel: 1,
            countyFeatureId: CountyId.Uppsala);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.UppsalaMunicipalityBbox, usePolygon: false);
        var response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeTrue();

        // Test 2 - User has permission for Uppsala county, search in Uppsala using polygon => Should return true
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.UppsalaMunicipalityBbox, usePolygon: true);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeTrue();

        // Test 3 - User has permission for Uppsala county, search in Uppsala using Area => Should return true
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, CountyId.Uppsala);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeTrue();

        // Test 4 - User has no permission for Jönköping county, search in Jönköping with bbox => Should return false        
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.JönköpingMunicipalityBbox, usePolygon: false);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();

        // Test 5 - User has no permission for Jönköping county, search in Jönköping with polygon => Should return false        
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.JönköpingMunicipalityBbox, usePolygon: true);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();

        // Test 6 - User has no permission for Jönköping county, search in Jönköping with Area => Should return false        
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, CountyId.Jönköping);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();

        // Test 7 - User has permission for Uppsala county, search in Tierp with bbox => Should return false since there are no observations in Tierp
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.TierpCenterBbox, usePolygon: false);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();

        // Test 8 - User has permission for Uppsala county, search in Tierp with polygon => Should return false since there are no observations in Tierp
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.TierpCenterBbox, usePolygon: true);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<bool>(TestContext.Current.CancellationToken);
        result.Should().BeFalse();

        // Test 9 - User has permission for Uppsala county, search in Tierp with bbox that intersects with an area where the user doesn't have access
        //       => Should return Http 409
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.TierpMunicipalityBbox, usePolygon: false);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Test 10 - User has no permissions for Jönköping county, search in Jönköping using Area => Should return http 403
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, CountyId.Jönköping);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Test 11 - User has no permissions for Jönköping county, search in Jönköping using bbox => Should return http 403
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.JönköpingMunicipalityBbox, usePolygon: false);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Test 12 - User has no permissions for Jönköping county, search in Jönköping using polygon => Should return http 403
        searchFilter = CreateSearchFilter(1999, sensitiveTaxonId, TestCoordinates.JönköpingMunicipalityBbox, usePolygon: true);
        response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true&returnHttp4xxWhenNoPermissions=true", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private SignalFilterDto CreateSearchFilter(int startYear, int taxonId, (double TopLeftLatitude, double TopLeftLongitude, double BottomRightLatitude, double BottomRightLongitude) bbox, bool usePolygon)
    {
        var searchFilter = new SignalFilterDto
        {
            StartDate = new DateTime(startYear, 1, 1),
            Geographics = new GeographicsFilterDto
            {
                Geometries = usePolygon ?
                [
                    new Polygon(
                    [
                        new LineString(
                        [
                            new Position(bbox.TopLeftLatitude, bbox.TopLeftLongitude),
                            new Position(bbox.TopLeftLatitude, bbox.BottomRightLongitude),
                            new Position(bbox.BottomRightLatitude, bbox.BottomRightLongitude),
                            new Position(bbox.BottomRightLatitude, bbox.TopLeftLongitude),
                            new Position(bbox.TopLeftLatitude, bbox.TopLeftLongitude)
                        ])
                    ])
                ] : null,
                BoundingBox = !usePolygon ? new LatLonBoundingBoxDto
                {                    
                    BottomRight = new LatLonCoordinateDto { Latitude = bbox.BottomRightLatitude, Longitude = bbox.BottomRightLongitude },
                    TopLeft = new LatLonCoordinateDto { Latitude = bbox.TopLeftLatitude, Longitude = bbox.TopLeftLongitude },
                } : null
            },
            Taxon = new TaxonFilterBaseDto
            {
                TaxonListIds = new[] { 1 },
                Ids = [taxonId]
            }
        };

        return searchFilter;
    }

    private SignalFilterDto CreateSearchFilter(int startYear, int taxonId, string countyId)
    {
        var searchFilter = new SignalFilterDto
        {
            StartDate = new DateTime(startYear, 1, 1),
            Geographics = new GeographicsFilterDto
            {
                Areas = new List<AreaFilterDto>
                {
                    new AreaFilterDto
                    {
                        AreaType = AreaTypeDto.County,
                        FeatureId = countyId
                    }
                }
            },
            Taxon = new TaxonFilterBaseDto
            {
                TaxonListIds = new[] { 1 },
                Ids = [taxonId]
            }
        };

        return searchFilter;
    }
}
