using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.Observations.Api.IntegrationTests.Utils;
using SOS.TestHelpers;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.TestDataTools
{
    public class CopyMongoDbCollectionTool : FixtureBase
    {        
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task CopyTaxonCollection()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fromClient = GetProcessClient(InstallationEnvironment.DevelopmentTest, "sos-dev");
            var toClient = GetProcessClient(InstallationEnvironment.Local, "sos");            
            var mongoDbUtil = new MongoDbUtil(fromClient, toClient);            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await mongoDbUtil.CopyCollectionAsync<Taxon>("Taxon", "Taxon_copy2");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------                        
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task CopyClamPortalObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fromClient = GetProcessClient(InstallationEnvironment.SystemTest, "sos-harvest-st");
            var toClient = GetProcessClient(InstallationEnvironment.Production, "sos-harvest");
            var mongoDbUtil = new MongoDbUtil(fromClient, toClient);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await mongoDbUtil.CopyCollectionAsync<ClamObservationVerbatim>("ClamObservationVerbatim", "ClamObservationVerbatim_copy");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------                        
        }

        private IProcessClient GetProcessClient(InstallationEnvironment environment, string databaseName = null)
        {
            var mongoDbConfiguration = GetMongoDbConfiguration(environment);
            // Change database name if specified. Otherwise use the one specified in appsettings.
            if (databaseName != null) mongoDbConfiguration.DatabaseName = databaseName;
            var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
            return processClient;
        }        

        protected MongoDbConfiguration GetMongoDbConfiguration(InstallationEnvironment environment)
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(environment);
            var mongoDbConfiguration = config.GetSection($"{configPrefix}:ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }
    }
}