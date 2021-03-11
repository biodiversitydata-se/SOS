using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Jobs;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.UnitTests.Managers
{
    public class ObservationsHarvestJobTests
    {
        private readonly Mock<IVocabulariesImportJob> _vocabularyImportJobMock;
        private readonly Mock<IVirtualHerbariumHarvestJob> _virtualHerbariumHarvestJobMock;
        private readonly Mock<ILogger<ObservationsHarvestJob>> _loggerMock;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ObservationsHarvestJobTests()
        {
          _vocabularyImportJobMock = new Mock<IVocabulariesImportJob>();
          _virtualHerbariumHarvestJobMock = new Mock<IVirtualHerbariumHarvestJob>();
            _loggerMock = new Mock<ILogger<ObservationsHarvestJob>>();
        }
    }
}