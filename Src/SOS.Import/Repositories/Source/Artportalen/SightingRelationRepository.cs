using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class SightingRelationRepository : BaseRepository<SightingRelationRepository>, ISightingRelationRepository
    {
        public SightingRelationRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<SightingRelationRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds, bool live = false)
        {
            try
            {
                const string query = @"
                SELECT	                
	                sr.SightingId,
	                sr.UserId,
	                sr.SightingRelationTypeId,
	                sr.Sort,
	                sr.IsPublic,	                
	                sr.DeterminationYear
                FROM
	                [SightingRelation] sr
                    INNER JOIN @tvp t ON sr.SightingId = t.Id AND sr.IsPublic = 1";

                return await QueryAsync<SightingRelationEntity>(query,
                    new {tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable")}, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting relations");
                return null;
            }
        }
    }
}