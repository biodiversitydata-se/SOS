using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ObservationDatabaseVerbatimRepository : VerbatimRepositoryBase<ObservationDatabaseVerbatim, int>,
        IObservationDatabaseVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ObservationDatabaseVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<ObservationDatabaseVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}