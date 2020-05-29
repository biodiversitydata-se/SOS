using System;
using Microsoft.Extensions.Logging;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Managers
{
    /// <summary>
    ///     Manager base
    /// </summary>
    public class ManagerBase<TEntity>
    {
        protected readonly ILogger<TEntity> Logger;
        protected readonly IProcessedObservationRepository ProcessRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public ManagerBase(
            IProcessedObservationRepository processedObservationRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processedObservationRepository ??
                                throw new ArgumentNullException(nameof(processedObservationRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}