using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.Fixtures
{
    [CollectionDefinition(Collections.ApiIntegrationTestsCollection)]
    public class ApiIntegrationTestCollection : ICollectionFixture<ApiIntegrationTestFixture>
    {

    }
}
