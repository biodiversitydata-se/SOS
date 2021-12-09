using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class TaxonRepository : BaseRepository<TaxonRepository>, ITaxonRepository
    {
        public TaxonRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<TaxonRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<TaxonEntity>> GetAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                Id,
	                SpeciesGroupId
                FROM 
	                Taxon
                WHERE 
	                SpeciesGroupId IS NOT NULL";

                return await QueryAsync<TaxonEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting taxa");
                return null;
            }
        }
    }
}