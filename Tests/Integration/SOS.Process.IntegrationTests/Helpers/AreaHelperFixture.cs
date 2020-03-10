using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;

namespace SOS.Process.IntegrationTests.Helpers
{
    public class AreaHelperFixture : TestBase, IDisposable
    {
        public AreaHelper AreaHelper { get; private set; }

        public AreaHelperFixture()
        {
            AreaHelper = CreateAreaHelper();
        }

        public void Dispose()
        {
            AreaHelper = null;
        }

        private AreaHelper CreateAreaHelper()
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var areaVerbatimRepository = new AreaVerbatimRepository(
                verbatimClient,
                new Mock<ILogger<AreaVerbatimRepository>>().Object);
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(
                processClient,
                new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper = new AreaHelper(
                areaVerbatimRepository,
                processedFieldMappingRepository);

            return areaHelper;
        }
    }
}