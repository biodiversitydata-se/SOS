using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SignalSearchIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SignalSearchIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_with_bounding_box()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fixture.UseUserServiceWithToken(_fixture.UserAuthenticationToken);

            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    BoundingBox = new LatLonBoundingBoxDto()
                    {
                        BottomRight = new LatLonCoordinateDto()
                        {
                            Latitude = -90, Longitude = 180
                        },
                        TopLeft = new LatLonCoordinateDto()
                        {
                            Latitude = 90, Longitude = -180
                        }
                    },
                    ConsiderObservationAccuracy = true,
                    ConsiderDisturbanceRadius = true,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies,
                        (int)TaxonListId.HabitatsDirective,
                        (int)TaxonListId.ProtectedByLaw,
                        (int)TaxonListId.ActionPlan,
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Jonkoping_county()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.JonkopingCounty)
                .Build();
            _fixture.UseMockUserService(authority);

            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = true,
                    ConsiderDisturbanceRadius = true,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies, 
                        (int)TaxonListId.HabitatsDirective, 
                        (int)TaxonListId.ProtectedByLaw, 
                        (int)TaxonListId.ActionPlan, 
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Ostergotland_county_and_consider_observation_accuracy()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.OstergotlandCounty)
                .Build();
            _fixture.UseMockUserService(authority);

            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = true,
                    ConsiderDisturbanceRadius = true,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies,
                        (int)TaxonListId.HabitatsDirective,
                        (int)TaxonListId.ProtectedByLaw,
                        (int)TaxonListId.ActionPlan,
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue("because when consider observation accuracy some observations in Östergötland could intersect Tranås municipality in Jönköping county.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Ostergotland_county_and_not_consider_observation_accuracy()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.OstergotlandCounty)
                .Build();
            _fixture.UseMockUserService(authority);

            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = false,
                    ConsiderDisturbanceRadius = false,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies,
                        (int)TaxonListId.HabitatsDirective,
                        (int)TaxonListId.ProtectedByLaw,
                        (int)TaxonListId.ActionPlan,
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_Lansstyrelsen()
        {            
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fixture.UseUserServiceWithToken(_fixture.UserAuthenticationToken);

            var searchFilter = new SignalFilterDto()
            {
                BirdNestActivityLimit = 19,                
                Geographics = new GeographicsFilterDto()
                {
                    Geometries = new List<IGeoShape>()
                    {
                        new PolygonGeoShape(new List<List<GeoCoordinate>> { new List<GeoCoordinate>
                            {
                                new GeoCoordinate(58.735828152014484, 17.100876661232803),
                                new GeoCoordinate(58.734512941657549, 17.102336549737057),
                                new GeoCoordinate(58.73246811074101, 17.102807815122453),
                                new GeoCoordinate(58.731512025975576, 17.100091710276246),
                                new GeoCoordinate(58.731616089143834, 17.094816153336815),
                                new GeoCoordinate(58.732355421320555, 17.092971703262172),
                                new GeoCoordinate(58.733167430011449, 17.092250913077425),
                                new GeoCoordinate(58.735250853649461, 17.093880296038122),
                                new GeoCoordinate(58.736054843437053, 17.09812663617927),
                                new GeoCoordinate(58.735828152014484, 17.100876661232803)
                            }
                        })
                    },
                    ConsiderObservationAccuracy = true,
                    ConsiderDisturbanceRadius = true,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies,
                        (int)TaxonListId.HabitatsDirective,
                        (int)TaxonListId.ProtectedByLaw,
                        (int)TaxonListId.ActionPlan,
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}