using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    public class OrganizationRepository : BaseRepository<OrganizationRepository>, Interfaces.IOrganizationRepository
    {
        public OrganizationRepository(
            ISpeciesPortalDataService speciesPortalDataService, 
            ILogger<OrganizationRepository> logger) : base(speciesPortalDataService, logger)
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
