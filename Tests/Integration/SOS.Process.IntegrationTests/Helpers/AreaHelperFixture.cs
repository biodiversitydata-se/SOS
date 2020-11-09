using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;

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
            var processedAreaRepository = new AreaRepository(
                processClient,
                new Mock<ILogger<AreaRepository>>().Object);
            var vocabularyRepository = new VocabularyRepository(
                processClient,
                new NullLogger<VocabularyRepository>());
            var areaHelper = new AreaHelper(
                processedAreaRepository,
                vocabularyRepository);

            return areaHelper;
        }
    }
}