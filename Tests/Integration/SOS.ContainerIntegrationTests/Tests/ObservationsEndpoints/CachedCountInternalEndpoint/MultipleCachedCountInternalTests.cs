using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Search.Result;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsEndpoints.CachedCountInternalEndpoint;

[Collection(TestCollection.Name)]
public class MultipleCachedCountInternalTests : TestBase
{
    public MultipleCachedCountInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task SumObservationCountInternalTest()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
           .All()
               .HaveValuesFromPredefinedObservations()
               .With(p => p.NotPresent = false)
               .With(p => p.NotRecovered = false)
               .With(p => p.UnsureDetermination = false)
           .TheFirst(20)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "1" })
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 15))
               .With(p => p.EndDate = new DateTime(2000, 1, 18))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "2" })
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 30))
               .With(p => p.EndDate = new DateTime(2000, 1, 30))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "3" })
           .TheNext(20)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 2, 1))
           .TheLast(20)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2000, 4, 1))
               .With(p => p.EndDate = new DateTime(2000, 4, 15))
           .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var taxonIds = new[] { 100011, 100012 };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/cachedcount", JsonContent.Create(taxonIds));
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TaxonSumAggregationItem>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(2);
        result!.Sum(r => r.ObservationCount).Should().Be(60,
            because: "");
    }
}