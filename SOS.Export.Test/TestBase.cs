using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Test.TestHelpers.JsonConverters;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Test
{
    public class TestBase
    {
        protected ExportConfiguration GetExportConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            ExportConfiguration exportConfiguration = config.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();
            return exportConfiguration;
        }

        protected Mock<IProcessedDarwinCoreRepository> CreateProcessedDarwinCoreRepositoryMock(string fileName)
        {
            var processedDarwinCoreRepositoryMock = new Mock<IProcessedDarwinCoreRepository>();
            var observations = LoadObservations(fileName);
            processedDarwinCoreRepositoryMock
                .Setup(pdcr => pdcr.GetChunkAsync(0, It.IsAny<int>()))
                .ReturnsAsync(observations);

            return processedDarwinCoreRepositoryMock;
        }

        protected Mock<IProcessedDarwinCoreRepository> CreateProcessedDarwinCoreRepositoryMock(DarwinCore<DynamicProperties> observation)
        {
            var processedDarwinCoreRepositoryMock = new Mock<IProcessedDarwinCoreRepository>();
            processedDarwinCoreRepositoryMock
                .Setup(pdcr => pdcr.GetChunkAsync(0, It.IsAny<int>()))
                .ReturnsAsync(new[] { observation });

            return processedDarwinCoreRepositoryMock;
        }

        protected DarwinCore<DynamicProperties> GetDefaultObservation()
        {
            string fileName = @"Resources\DefaultTestObservation.json";
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observation = JsonConvert.DeserializeObject<DarwinCore<DynamicProperties>>(str, serializerSettings);
            return observation;
        }

        private IEnumerable<DarwinCore<DynamicProperties>> LoadObservations(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observations = JsonConvert.DeserializeObject<List<DarwinCore<DynamicProperties>>>(str, serializerSettings);
            return observations;
        }
    }
}
