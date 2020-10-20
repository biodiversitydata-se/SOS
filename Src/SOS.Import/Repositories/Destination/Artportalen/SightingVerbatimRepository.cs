using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Repositories.Destination.Artportalen
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class SightingVerbatimRepositoryOld : VerbatimRepository<ArtportalenObservationVerbatim, int>,
        ISightingVerbatimRepositoryOld
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public SightingVerbatimRepositoryOld(
            IVerbatimClient importClient,
            ILogger<SightingVerbatimRepositoryOld> logger) : base(importClient, logger)
        {
        }

        /// <inheritdoc />
        public async Task<int> GetMaxIdAsync()
        {
            try
            {
                var res = await MongoCollection
                    .Find(FilterDefinition<ArtportalenObservationVerbatim>.Empty)
                    .Project(s => s.Id)
                    .Limit(1)
                    .Sort(Builders<ArtportalenObservationVerbatim>.Sort.Descending(s => s.Id))
                    .FirstOrDefaultAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }
    }
}