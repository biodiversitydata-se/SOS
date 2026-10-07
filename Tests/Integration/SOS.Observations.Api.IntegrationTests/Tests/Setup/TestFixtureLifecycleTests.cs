using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;

namespace SOS.Observations.Api.IntegrationTests.Tests.Setup;

public class TestFixtureLifecycleTests
{
    [Fact]
    public async Task Fixtures_CanBeDisposedBeforeInitialization_AndMoreThanOnce()
    {
        var containers = new TestContainersFixture();
        await containers.DisposeAsync();
        await containers.DisposeAsync();

        var fixture = new TestFixture();
        await fixture.DisposeAsync();
        await fixture.DisposeAsync();
    }
}
