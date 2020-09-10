using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Database base class
    /// </summary>
    public class BaseRepository<T> : IBaseRepository<T>
    {
        private readonly IArtportalenDataService _artportalenDataService;

        /// <summary>
        ///     Logger
        /// </summary>
        protected ILogger<T> Logger { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public BaseRepository(IArtportalenDataService artportalenDataService, ILogger<T> logger)
        {
            _artportalenDataService =
                artportalenDataService ?? throw new ArgumentNullException(nameof(artportalenDataService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters = null, bool live = false)
        {
            return await _artportalenDataService.QueryAsync<E>(query, parameters, live);
        }
    }
}