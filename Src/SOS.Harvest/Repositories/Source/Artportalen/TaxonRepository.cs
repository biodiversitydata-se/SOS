using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;

namespace SOS.Harvest.Repositories.Source.Artportalen
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
                throw;
            }
        }
    }
}