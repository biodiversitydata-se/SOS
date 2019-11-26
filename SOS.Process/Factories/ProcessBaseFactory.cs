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
        protected readonly IDarwinCoreRepository ProcessRepository;
        protected readonly ILogger<TEntity> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="DarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public ProcessBaseFactory(
            IDarwinCoreRepository DarwinCoreRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = DarwinCoreRepository ?? throw new ArgumentNullException(nameof(DarwinCoreRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
