using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Harvesters.iNaturalist;
using SOS.Harvest.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class iNaturalistObservationHarvesterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestTenThousandObservations_FromiNaturalistProvider_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.iNaturalistServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested = 100000;

            var iNaturalistObservationService = new iNaturalistObservationService(
                new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), 
                importConfiguration.iNaturalistServiceConfiguration,
                new NullLogger<iNaturalistObservationService>());
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var verbatimClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var iNaturalistObservationHarvester = new iNaturalistObservationHarvester(
                verbatimClient,
                iNaturalistObservationService,
                importConfiguration.iNaturalistServiceConfiguration,
                new Mock<ILogger<iNaturalistObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await iNaturalistObservationHarvester.HarvestObservationsAsync(JobRunModes.Full, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestTenThousandObservations_FromiNaturalistProvider_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.iNaturalistServiceConfiguration.StartHarvestYear = 2015;
            importConfiguration.iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested = 10000;
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var verbatimClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize); ;

            var iNaturalistObservationHarvester = new iNaturalistObservationHarvester(
                verbatimClient,
                new iNaturalistObservationService(
                    new HttpClientService(new Mock<ILogger<HttpClientService>>().Object),
                    importConfiguration.iNaturalistServiceConfiguration,
                    new NullLogger<iNaturalistObservationService>()),
                importConfiguration.iNaturalistServiceConfiguration,
                new Mock<ILogger<iNaturalistObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await iNaturalistObservationHarvester.HarvestObservationsAsync(JobRunModes.Full, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}