using ContractGeographicsFilterDto = SOS.Api.Contracts.Observations.Requests.GeographicsFilterDto;
using ContractOccurrenceStatusFilterValuesDto = SOS.Api.Contracts.Observations.Requests.OccurrenceStatusFilterValuesDto;
using ContractSearchFilterAggregationInternalDto = SOS.Api.Contracts.Aggregations.Requests.SearchFilterAggregationInternalDto;
using ContractTaxonFilterDto = SOS.Api.Contracts.Observations.Requests.TaxonFilterDto;
using FizzWare.NBuilder;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiClient;

[Collection(TestCollection.Name)]
[Trait("Category", "ReadOnlyIntegrationTest")]
public class LookupServiceIntegrationTests : TestBase
{
    public LookupServiceIntegrationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }        

    [Fact]
    public async Task LookupClients_ReturnLookupData_FromObservationsApi()
    {
        await TestFixture.InitializeAreasAsync();
        var vocabulariesClient = TestFixture.CreateSosApiClient<ISosVocabulariesClient>();
        var dataProvidersClient = TestFixture.CreateSosApiClient<ISosDataProvidersClient>();
        var areasClient = TestFixture.CreateSosApiClient<ISosAreasClient>();

        var vocabularies = await vocabulariesClient.GetVocabulariesAsync(TestContext.Current.CancellationToken);
        var dataProviders = await dataProvidersClient.GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);
        var areas = await areasClient.GetAreasAsync(
            new AreasOptions { Take = 1 },
            TestContext.Current.CancellationToken);

        vocabularies.Should().NotBeEmpty();
        dataProviders.Should().NotBeNull();
        areas.Should().NotBeNull();
        areas!.Records.Should().NotBeNull();
    }    
}
