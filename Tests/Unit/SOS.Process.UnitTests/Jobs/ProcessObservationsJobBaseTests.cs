using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Jobs;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs;

public class ProcessObservationsJobBaseTests
{
    [Fact]
    public async Task GetTaxaAsyncContinuesWhenArtportalenTaxaProcessingFails()
    {
        var processTaxaJob = new Mock<IProcessTaxaJob>();
        processTaxaJob.Setup(job => job.RunAsync()).ReturnsAsync(true);

        var processArtportalenTaxaJob = new Mock<IProcessArtportalenTaxaJob>();
        processArtportalenTaxaJob
            .Setup(job => job.RunAsync())
            .ThrowsAsync(new InvalidOperationException("Artportalen taxonomy failed"));

        var taxonCache = new Mock<ICache<int, Taxon>>();
        taxonCache.Setup(cache => cache.ClearAsync()).Returns(Task.CompletedTask);
        taxonCache.Setup(cache => cache.GetAllAsync()).ReturnsAsync(
            new[]
            {
                new Taxon { Id = 1, IsNotAccepted = false }
            });

        var logger = new Mock<ILogger<ProcessObservationsJobBase>>();
        var job = new TestableProcessObservationsJob(
            taxonCache.Object,
            processTaxaJob.Object,
            processArtportalenTaxaJob.Object,
            logger.Object);

        var taxa = await job.GetTaxaForTestAsync();

        taxa.Should().ContainKey(1);
        processTaxaJob.Verify(processJob => processJob.RunAsync(), Times.Once);
        processArtportalenTaxaJob.Verify(processJob => processJob.RunAsync(), Times.Once);
        taxonCache.Verify(cache => cache.ClearAsync(), Times.Once);
        logger.Verify(log => log.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) =>
                state.ToString() == "Failed to process Artportalen taxa. Observation processing will continue."),
            It.Is<Exception>(exception => exception.Message == "Artportalen taxonomy failed"),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    private sealed class TestableProcessObservationsJob : ProcessObservationsJobBase
    {
        public TestableProcessObservationsJob(
            ICache<int, Taxon> taxonCache,
            IProcessTaxaJob processTaxaJob,
            IProcessArtportalenTaxaJob processArtportalenTaxaJob,
            ILogger<ProcessObservationsJobBase> logger)
            : base(
                Mock.Of<IProcessedObservationCoreRepository>(),
                Mock.Of<IProcessInfoRepository>(),
                Mock.Of<IHarvestInfoRepository>(),
                Mock.Of<IObservationProcessorManager>(),
                taxonCache,
                Mock.Of<ICache<VocabularyId, Vocabulary>>(),
                Mock.Of<IDataProviderCache>(),
                Mock.Of<IProcessTimeManager>(),
                Mock.Of<IValidationManager>(),
                processTaxaJob,
                processArtportalenTaxaJob,
                Mock.Of<IAreaHelper>(),
                Mock.Of<ProcessConfiguration>(),
                logger)
        {
        }

        public Task<IDictionary<int, Taxon>> GetTaxaForTestAsync()
        {
            return GetTaxaAsync(JobRunModes.Full);
        }
    }
}
