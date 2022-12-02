using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class PersonRepository : BaseRepository<PersonRepository>, IPersonRepository
    {
        private const string _query = @"
            SELECT 
	            p.Id,
	            u.Id as UserId,	                
	            p.FirstName,
	            p.LastName,
                u.UserAlias AS Alias,
                u.UserServiceUserId
            FROM 
	            [Person] p
                INNER JOIN [User] u ON p.Id = u.PersonId";

        public PersonRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<PersonRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<PersonEntity>> GetAsync()
        {
            try
            {
                return await QueryAsync<PersonEntity>(_query, null!);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting persons");
                return null!;
            }
        }

        public async Task<PersonEntity> GetAsync(int id)
        {
            try
            {
                return (await QueryAsync<PersonEntity>(_query, null!))?.FirstOrDefault()!;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error getting person: {id} ");
                return null!;
            }
        }
    }
}