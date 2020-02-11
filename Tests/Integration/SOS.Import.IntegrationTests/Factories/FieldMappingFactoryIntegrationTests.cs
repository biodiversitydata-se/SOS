using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Import.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Factories
{
    public class FieldMappingFactoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task Imports_FieldMappings_To_MongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            var fieldMappingResourceRepository = new FieldMappingRepository(
                new ImportClient(
                    importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.VerbatimDbConfiguration.DatabaseName,
                    importConfiguration.VerbatimDbConfiguration.BatchSize), 
                new NullLogger<FieldMappingRepository>());

            var fieldMappingFactory = new FieldMappingFactory(
                fieldMappingResourceRepository,
                new NullLogger<FieldMappingFactory>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await fieldMappingFactory.ImportAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}