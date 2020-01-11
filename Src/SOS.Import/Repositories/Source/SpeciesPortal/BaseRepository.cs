using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Database base class
    /// </summary>
    public class BaseRepository<T> : Interfaces.IBaseRepository<T>
    {
        private readonly ISpeciesPortalDataService _speciesPortalDataService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public BaseRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<T> logger)
        {
            _speciesPortalDataService = speciesPortalDataService ?? throw new ArgumentNullException(nameof(speciesPortalDataService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger<T> Logger { get; private set; }

        /// <summary>
        /// Query data base
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters = null)
        {
            return await _speciesPortalDataService.QueryAsync<E>(query, parameters);
        }
    }
}
