using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Database;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Destination;

namespace SOS.Process.IntegrationTests.Helpers
{
    public class AreaHelperFixture : TestBase, IDisposable
    {
        public AreaHelperFixture()
        {
            AreaHelper = CreateAreaHelper();
        }

        public AreaHelper AreaHelper { get; private set; }

        public void Dispose()
        {
            AreaHelper = null;
        }

        private AreaHelper CreateAreaHelper()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedAreaRepository = new ProcessedAreaRepository(
                processClient,
                new Mock<ILogger<ProcessedAreaRepository>>().Object);
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(
                processClient,
                new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper = new AreaHelper(
                processedAreaRepository,
                processedFieldMappingRepository);

            return areaHelper;
        }
    }
}