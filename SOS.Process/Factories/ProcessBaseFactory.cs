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
        /// <param name="darwinCoreRepository"></param>
        /// <param name="logger"></param>
        public ProcessBaseFactory(
            IDarwinCoreRepository darwinCoreRepository,
            ILogger<TEntity> logger)
        {
            ProcessRepository = darwinCoreRepository ?? throw new ArgumentNullException(nameof(darwinCoreRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
