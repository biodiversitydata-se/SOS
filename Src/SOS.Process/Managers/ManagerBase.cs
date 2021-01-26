using System;
using Microsoft.Extensions.Logging;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Process.Managers
{
    /// <summary>
    ///     Manager base
    /// </summary>
    public class ManagerBase<TEntity>
    {
        protected readonly ILogger<TEntity> Logger;
        protected readonly IProcessedPublicObservationRepository PublicProcessRepository;
        protected readonly IProcessedProtectedObservationRepository ProtectedProcessRepository;
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="logger"></param>
        public ManagerBase(
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IProcessedProtectedObservationRepository processedProtectedObservationRepository,
            ILogger<TEntity> logger)
        {
            PublicProcessRepository = processedPublicObservationRepository ??
                                      throw new ArgumentNullException(nameof(processedPublicObservationRepository));
            ProtectedProcessRepository = processedProtectedObservationRepository ??
                                         throw new ArgumentNullException(
                                             nameof(processedProtectedObservationRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}