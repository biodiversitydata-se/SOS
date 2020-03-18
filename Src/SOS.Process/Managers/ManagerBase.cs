using System;
using Microsoft.Extensions.Logging;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Managers
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ManagerBase<TEntity>
    {
        protected readonly IProcessedObservationRepository ProcessRepository;
        protected readonly ILogger<TEntity> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public ManagerBase(
            IProcessedObservationRepository processedObservationRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
