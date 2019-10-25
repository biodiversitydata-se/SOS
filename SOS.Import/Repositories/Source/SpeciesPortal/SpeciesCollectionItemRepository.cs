using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    public class SpeciesCollectionItemRepository : BaseRepository<SpeciesCollectionItemRepository>, Interfaces.ISpeciesCollectionItemRepository
    {
        public SpeciesCollectionItemRepository(
            ISpeciesPortalDataService speciesPortalDataService, 
            ILogger<SpeciesCollectionItemRepository> logger) : base(speciesPortalDataService, logger)
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
