using FizzWare.NBuilder;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.Stubs;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsEndpoints.SignalSearchInternalEndpoint;

[Collection(TestCollection.Name)]
public class SignalSearchTests : TestBase
{
    public SignalSearchTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task SignalSearchInternalTest()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All()
                .HaveValuesFromPredefinedObservations(true)
            .TheFirst(20)
                .With(p => p.TaxonId = 100051)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
            .TheNext(20)
                .With(p => p.TaxonId = 100057)
                .With(p => p.StartDate = new DateTime(2000, 1, 15))
                .With(p => p.EndDate = new DateTime(2000, 1, 18))
            .TheNext(20)
                .With(p => p.TaxonId = 100069)
                .With(p => p.StartDate = new DateTime(2000, 1, 30))
                .With(p => p.EndDate = new DateTime(2000, 1, 30))
            .TheNext(20)
                .With(p => p.TaxonId = 100070)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 2, 1))
            .TheLast(20)
                .With(p => p.TaxonId = 100051)
                .With(p => p.StartDate = new DateTime(1999, 4, 1))
                .With(p => p.EndDate = new DateTime(1999, 4, 15))
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithCountySightingIndicationAuthority(
            maxProtectionLevel: 1,
            countyFeatureId: "3");
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
       
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
                TaxonListIds = new[] { 1 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/signalsearch?onlyAboveMyClearance=true", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();        
    }
}