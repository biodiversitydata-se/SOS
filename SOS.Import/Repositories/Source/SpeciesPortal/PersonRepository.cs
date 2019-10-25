using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    public class PersonRepository : BaseRepository<PersonRepository>, Interfaces.IPersonRepository
    {
        public PersonRepository(
            ISpeciesPortalDataService speciesPortalDataService, 
            ILogger<PersonRepository> logger) : base(speciesPortalDataService, logger)
        {
        }

        public async Task<IEnumerable<PersonEntity>> GetAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                p.Id,
	                u.Id as UserId,	                
	                p.FirstName,
	                p.LastName
                FROM 
	                [Person] p
                    INNER JOIN [User] u ON p.Id = u.PersonId";

                return await QueryAsync<PersonEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting persons");
                return null;
            }
        }
    }
}
