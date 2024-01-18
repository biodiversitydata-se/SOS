using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ExportsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ExportToGeoJsonInternalIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ExportToGeoJsonInternalIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_Sensitive_Observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fixture.UseUserServiceWithToken(_fixture.UserAuthenticationToken);
            var searchFilter = new SearchFilterInternalDto()
            {
                ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
                DataProvider = new DataProviderFilterDto
                {
                    Ids = new List<int> {
                        1,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        15,
                        16,
                        17,
                        19,
                        20,
                        21,
                        22,
                        23,
                        24
                    }
                },
                Taxon = new TaxonFilterDto
                {
                    IncludeUnderlyingTaxa = false,
                    Ids = new List<int>
                    {
                        101510,
                        259037,
                        259036,
                        259035,
                        259038
                    }
                },
                NotRecoveredFilter = SightingNotRecoveredFilterDto.DontIncludeNotRecovered,
                ExtendedFilter = new ExtendedFilterDto
                {
                    SightingTypeSearchGroupIds = new List<int>
                    {
                        1,
                        4,
                        16,
                        32,
                        128
                    }
                },
                DeterminationFilter = SightingDeterminationFilterDto.NotUnsureDetermination,
                Output = new OutputFilterExtendedDto
                {
                    Fields = new List<string>
                    {
                        "ArtportalenInternal.SightingTypeId",
                        "DataProviderId",
                        "Event.EndDate",
                        "Event.StartDate",
                        "Identification.ConfirmedBy",
                        "Identification.IdentifiedBy",
                        "Identification.UncertainIdentification",
                        "Identification.VerificationStatus",
                        "Identification.Verified",
                        "IsSensitive",
                        "Location.CoordinateUncertaintyInMeters",
                        "Location.County",
                        "Location.DecimalLatitude",
                        "Location.DecimalLongitude",
                        "Location.Etrs89X",
                        "Location.Etrs89Y",
                        "Location.Locality",
                        "Location.Municipality",
                        "Location.Parish",
                        "Location.Province",
                        "Location.Sweref99TmX",
                        "Location.Sweref99TmY",
                        "Occurrence.Activity",
                        "Occurrence.Biotope",
                        "Occurrence.IndividualCount",
                        "Occurrence.IsNeverFoundObservation",
                        "Occurrence.IsNotRediscoveredObservation",
                        "Occurrence.IsPositiveObservation",
                        "Occurrence.LifeStage",
                        "Occurrence.OccurrenceId",
                        "Occurrence.OccurrenceRemarks",
                        "Occurrence.OccurrenceStatus",
                        "Occurrence.OrganismQuantity",
                        "Occurrence.OrganismQuantityUnit",
                        "Occurrence.RecordedBy",
                        "Occurrence.ReportedBy",
                        "Occurrence.SensitivityCategory",
                        "Occurrence.Sex",
                        "Occurrence.Substrate",
                        "Projects",
                        "PublicCollection",
                        "RightsHolder",
                        "SpeciesCollectionLabel",
                        "Taxon.Attributes.IsRedlisted",
                        "Taxon.Attributes.OrganismGroup",
                        "Taxon.Attributes.ProtectedByLaw",
                        "Taxon.Attributes.RedlistCategory",
                        "Taxon.Attributes.SortOrder",
                        "Taxon.Id",
                        "Taxon.ScientificName",
                        "Taxon.VernacularName"
                    }
                },
                IncludeRealCount = true
            };


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadGeoJsonInternalAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
                PropertyLabelType.Swedish,
                "sv-SE",
                true);
            var bytes = response.GetFileContentResult();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bytes.Length.Should().BeGreaterThan(0);

            var filename = FilenameHelper.CreateFilenameWithDate("geojson_export", "zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }
    }
}