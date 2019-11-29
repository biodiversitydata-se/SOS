using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class KulObservationFactoryIntegrationTests : TestBase
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

            var kulObservationService = new KulObservationService(
                new Mock<ILogger<KulObservationService>>().Object, 
                importConfiguration.KulServiceConfiguration);
            
            var kulObservationVerbatimRepository = new KulObservationVerbatimRepository(
                new ImportClient(
                    importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.MongoDbConfiguration.DatabaseName,
                    importConfiguration.MongoDbConfiguration.BatchSize), 
                new Mock<ILogger<KulObservationVerbatimRepository>>().Object);

        var kulObservationFactory = new KulObservationFactory(
                kulObservationService,
                kulObservationVerbatimRepository, 
                importConfiguration.KulServiceConfiguration,
                GetHarvestInfoRepository(),
                new Mock<ILogger<KulObservationFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulObservationFactory.HarvestObservationsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
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
           
            var kulObservationFactory = new KulObservationFactory(
                new KulObservationService(
                    new Mock<ILogger<KulObservationService>>().Object,
                    importConfiguration.KulServiceConfiguration),
                new Mock<IKulObservationVerbatimRepository>().Object,
                importConfiguration.KulServiceConfiguration,
                GetHarvestInfoRepository(),
                new Mock<ILogger<KulObservationFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulObservationFactory.HarvestObservationsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private IHarvestInfoRepository GetHarvestInfoRepository()
        {
            var harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            harvestInfoRepositoryMock.Setup(hir =>
                    hir.UpdateHarvestInfoAsync(It.IsAny<string>(), DataProviderId.ClamAndTreePortal, It.IsAny<int>()))
                .ReturnsAsync(true);

            return harvestInfoRepositoryMock.Object;
        }
    }
}