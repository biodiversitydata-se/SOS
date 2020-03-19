using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class SpeciesCollectionItemRepository : BaseRepository<SpeciesCollectionItemRepository>, Interfaces.ISpeciesCollectionItemRepository
    {
        public SpeciesCollectionItemRepository(
            IArtportalenDataService artportalenDataService, 
            ILogger<SpeciesCollectionItemRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<SpeciesCollectionItemEntity>> GetAsync()
        {
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
                FROM [SightingSpeciesCollectionItem] ssci";

                return await QueryAsync<SpeciesCollectionItemEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Species Collection Items");
                return null;
            }
        }
    }
}
