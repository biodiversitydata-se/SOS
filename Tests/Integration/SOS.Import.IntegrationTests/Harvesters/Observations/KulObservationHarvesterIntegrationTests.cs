using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services;
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
            var importConfiguration = GetImportConfiguration();
            importConfiguration.KulServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested = 10000;

            var kulObservationService = new KulObservationService(
                new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), 
                importConfiguration.KulServiceConfiguration,
                new NullLogger<KulObservationService>());

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
            var importConfiguration = GetImportConfiguration();
            importConfiguration.KulServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested = 10000;

            var kulObservationHarvester = new KulObservationHarvester(
                new KulObservationService(
                    new HttpClientService(new Mock<ILogger<HttpClientService>>().Object),
                    importConfiguration.KulServiceConfiguration,
                    new NullLogger<KulObservationService>()),
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