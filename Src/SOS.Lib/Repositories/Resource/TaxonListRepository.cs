using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Taxon list repository.
    /// </summary>
    public class TaxonListRepository : RepositoryBase<TaxonList, int>, ITaxonListRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public TaxonListRepository(
            IProcessClient processClient,
            ILogger<TaxonListRepository> logger) : base(processClient, logger)
        {
        }
    }
}