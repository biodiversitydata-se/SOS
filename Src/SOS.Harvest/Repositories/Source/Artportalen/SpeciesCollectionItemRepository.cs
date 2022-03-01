using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class SpeciesCollectionItemRepository : BaseRepository<SpeciesCollectionItemRepository>,
        ISpeciesCollectionItemRepository
    {
        public SpeciesCollectionItemRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<SpeciesCollectionItemRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<SpeciesCollectionItemEntity>> GetBySightingAsync(IEnumerable<int> sightingIds, bool live = false)
        {
            if (!sightingIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                const string query = @"
                SELECT
                    ssci.OrganizationId,
                    ssci.CollectorId,
                    ssci.SightingId,      
                    ssci.Description,      
                    ssci.DeterminerText,
                    ssci.ConfirmatorText,
                    ssci.DeterminerYear,
                    ssci.ConfirmatorYear
                FROM [SightingSpeciesCollectionItem] ssci
                INNER JOIN @tvp t ON ssci.SightingId = t.Id";

                return await QueryAsync<SpeciesCollectionItemEntity>(query, new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Species Collection Items");
                return null;
            }
        }
    }
}