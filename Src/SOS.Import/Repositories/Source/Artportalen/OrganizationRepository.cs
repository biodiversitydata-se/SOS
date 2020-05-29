using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class OrganizationRepository : BaseRepository<OrganizationRepository>, IOrganizationRepository
    {
        public OrganizationRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<OrganizationRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<OrganizationEntity>> GetAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                Id,
                    OrganizationId,
	                Name
                FROM
                    [Organization]";

                return await QueryAsync<OrganizationEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting organizations");
                return null;
            }
        }
    }
}