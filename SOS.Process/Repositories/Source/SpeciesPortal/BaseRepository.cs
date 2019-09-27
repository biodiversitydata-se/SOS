using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Process.Services.Interfaces;

namespace SOS.Process.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Database base class
    /// </summary>
    public class BaseRepository<T> : Interfaces.IBaseRepository<T>
    {
        private readonly ISpeciesPortalDataService _SpeciesPortalDataService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public BaseRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<T> logger)
        {
            _SpeciesPortalDataService = SpeciesPortalDataService ?? throw new ArgumentNullException(nameof(SpeciesPortalDataService));
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
            return await _SpeciesPortalDataService.QueryAsync<E>(query, parameters);
        }
    }
}
