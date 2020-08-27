using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class SpeciesCollectionItemRepository : BaseRepository<SpeciesCollectionItemRepository>,
        ISpeciesCollectionItemRepository
    {
        public SpeciesCollectionItemRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<SpeciesCollectionItemRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<SpeciesCollectionItemEntity>> GetBySightingAsync(IEnumerable<int> sightingIds)
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

                return await QueryAsync<SpeciesCollectionItemEntity>(query, new { tvp = sightingIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Species Collection Items");
                return null;
            }
        }
    }
}