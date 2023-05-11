using Xunit;

namespace SOS.AutomaticIntegrationTests.TestFixtures
{
    [CollectionDefinition(Constants.IntegrationTestsCollectionName)]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {

    }
}
