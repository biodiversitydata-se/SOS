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
        protected readonly IProcessedRepository ProcessRepository;

        protected readonly ILogger<TEntity> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processRepository"></param>
        /// <param name="logger"></param>
        public ProcessBaseFactory(
            IProcessedRepository processRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
