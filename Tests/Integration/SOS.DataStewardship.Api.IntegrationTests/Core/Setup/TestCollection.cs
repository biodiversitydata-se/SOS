namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup;

[CollectionDefinition(Constants.IntegrationTestsCollectionName, DisableParallelization = true)]
public class TestCollection : ICollectionFixture<TestFixture>
{

}