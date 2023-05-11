using Microsoft.Extensions.Logging;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Harvest.Managers
{
    /// <summary>
    ///     Manager base
    /// </summary>
    public class ManagerBase<TEntity>
    {
        protected readonly ILogger<TEntity> Logger;
        protected readonly IProcessedObservationCoreRepository ProcessedObservationRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public ManagerBase(
            IProcessedObservationCoreRepository processedObservationRepository,
            ILogger<TEntity> logger)
        {
            ProcessedObservationRepository = processedObservationRepository ??
                                             throw new ArgumentNullException(nameof(processedObservationRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}