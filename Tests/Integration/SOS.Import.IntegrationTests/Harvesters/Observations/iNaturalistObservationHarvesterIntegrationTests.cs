using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
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

            var kulObservationService = new iNaturalistObservationService(
                new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), 
                importConfiguration.iNaturalistServiceConfiguration,
                new NullLogger<iNaturalistObservationService>());

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var dwcObservationVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                new VerbatimClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<DarwinCoreArchiveVerbatimRepository>>().Object);

            var iNaturalistObservationHarvester = new iNaturalistObservationHarvester(
                kulObservationService,
                dwcObservationVerbatimRepository,
                importConfiguration.iNaturalistServiceConfiguration,
                new Mock<ILogger<iNaturalistObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await iNaturalistObservationHarvester.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);

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

            var iNaturalistObservationHarvester = new iNaturalistObservationHarvester(
                new iNaturalistObservationService(
                    new HttpClientService(new Mock<ILogger<HttpClientService>>().Object),
                    importConfiguration.iNaturalistServiceConfiguration,
                    new NullLogger<iNaturalistObservationService>()),
                new Mock<IDarwinCoreArchiveVerbatimRepository>().Object,
                importConfiguration.iNaturalistServiceConfiguration,
                new Mock<ILogger<iNaturalistObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await iNaturalistObservationHarvester.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}