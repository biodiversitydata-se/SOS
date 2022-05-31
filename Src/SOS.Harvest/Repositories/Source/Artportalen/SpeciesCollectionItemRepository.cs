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

        ///<inheritdoc />
        public async Task<IEnumerable<SpeciesCollectionItemEntity>?> GetBySightingAsync(IEnumerable<int> sightingIds)
        {
            if (!sightingIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                const string query = @"
                SELECT
                    ssci.CollectorId,
                    ssci.ConfirmatorText,
                    ssci.ConfirmatorYear,
                    ssci.Description,      
                    ssci.DeterminerText,
                    ssci.DeterminerYear,
                    ssci.Id,
                    ssci.[Label],
                    ssci.OrganizationId,
                    ssci.SightingId
                FROM [SightingSpeciesCollectionItem] ssci
                INNER JOIN @tvp t ON ssci.SightingId = t.Id";

                return await QueryAsync<SpeciesCollectionItemEntity>(query, new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Species Collection Items");
                return null;
            }
        }
    }
}