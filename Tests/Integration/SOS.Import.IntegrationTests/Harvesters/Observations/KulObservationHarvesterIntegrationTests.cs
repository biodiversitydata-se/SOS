using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using KulService;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class KulObservationHarvesterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestTenThousandObservations_FromKulProvider_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            importConfiguration.KulServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested = 10000;
            var speciesObservationChangeServiceClient = new SpeciesObservationChangeServiceClient();

            var kulObservationService = new KulObservationService(
                speciesObservationChangeServiceClient,
                importConfiguration.KulServiceConfiguration,
                new Mock<ILogger<KulObservationService>>().Object);
            
            var kulObservationVerbatimRepository = new KulObservationVerbatimRepository(
                new ImportClient(
                    importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.VerbatimDbConfiguration.DatabaseName,
                    importConfiguration.VerbatimDbConfiguration.BatchSize), 
                new Mock<ILogger<KulObservationVerbatimRepository>>().Object);

        var kulObservationHarvester = new KulObservationHarvester(
                kulObservationService,
                kulObservationVerbatimRepository, 
                importConfiguration.KulServiceConfiguration,
                new Mock<ILogger<KulObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulObservationHarvester.HarvestObservationsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestTenThousandObservations_FromKulProvider_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            importConfiguration.KulServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested = 10000;
            var speciesObservationChangeServiceClient = new SpeciesObservationChangeServiceClient();

            var kulObservationHarvester = new KulObservationHarvester(
                new KulObservationService(
                    speciesObservationChangeServiceClient,
                    importConfiguration.KulServiceConfiguration,
                    new Mock<ILogger<KulObservationService>>().Object),
                new Mock<IKulObservationVerbatimRepository>().Object,
                importConfiguration.KulServiceConfiguration,
                new Mock<ILogger<KulObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulObservationHarvester.HarvestObservationsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}