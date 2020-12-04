using Xunit;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{
    [CollectionDefinition("Api test collection")]
    public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
    {
    }
}