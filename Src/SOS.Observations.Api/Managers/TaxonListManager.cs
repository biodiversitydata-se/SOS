using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Taxon list manager.
    /// </summary>
    public class TaxonListManager : ITaxonListManager
    {
        private readonly ILogger<TaxonListManager> _logger;
        private readonly ICache<int, TaxonList> _taxonListCache;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonListCache"></param>
        /// <param name="logger"></param>
        public TaxonListManager(
            ICache<int, TaxonList> taxonListCache,
            ILogger<TaxonListManager> logger)
        {
            _taxonListCache = taxonListCache ??
                              throw new ArgumentNullException(nameof(taxonListCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<TaxonList>> GetTaxonListsAsync()
        {
            return await _taxonListCache.GetAllAsync();
        }
    }
}