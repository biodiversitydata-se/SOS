using System;
using Microsoft.Extensions.Logging;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ProcessBaseFactory<TEntity>
    {
        protected readonly IProcessedSightingRepository ProcessRepository;
        protected readonly ILogger<TEntity> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedSightingRepository"></param>
        /// <param name="logger"></param>
        public ProcessBaseFactory(
            IProcessedSightingRepository processedSightingRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processedSightingRepository ?? throw new ArgumentNullException(nameof(processedSightingRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
