namespace SOS.Observations.Api.IntegrationTests.Setup;

/// <summary>
/// Base class for tests that write Elasticsearch data. Resets the shared indices
/// before every test; tests that do not write data use <see cref="TestBase"/>.
/// </summary>
public class ElasticsearchTestBase : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchTestBase"/> class.
    /// </summary>
    /// <param name="testFixture">The <see cref="Setup.TestFixture"/> instance for setting up the integration test environment.</param>
    /// <param name="output">The <see cref="ITestOutputHelper"/> instance for test logging and output in the test runner.</param>
    public ElasticsearchTestBase(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    public async ValueTask InitializeAsync()
    {
        await ProcessFixture.CleanElasticsearchIndices();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
