﻿using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class ChecklistRepository : BaseRepository<IChecklistRepository>, IChecklistRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public ChecklistRepository(IArtportalenDataService artportalenDataService, ILogger<ChecklistRepository> logger) :
            base(artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, ICollection<int>>?> GetChecklistsTaxonIdsAsync(
            IEnumerable<int> checklistIds)
        {
            var query = $@"
	        SELECT 
		        clt.[ChecklistId],
		        clt.[TaxonId]
	        FROM 
		        ChecklistTaxon clt
                INNER JOIN @tvp t ON clt.ChecklistId = t.Id";

            var result = await QueryAsync<(int checklistId, int taxonId)>(query,
                new { tvp = checklistIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

            if (!result?.Any() ?? true)
            {
                return null;
            }

            var checklistsTaxa = new Dictionary<int, ICollection<int>>();

            foreach (var item in result!)
            {
                if (!checklistsTaxa.TryGetValue(item.checklistId, out var checklistTaxa))
                {
                    checklistTaxa = new HashSet<int>();
                    checklistsTaxa.Add(item.checklistId, checklistTaxa);
                }
                checklistTaxa.Add(item.taxonId);
            }

            return checklistsTaxa;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ChecklistEntity>> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = @$"
                    SELECT  
	                cl.[Id],
	                cl.[ControlingUserId],
	                cl.[Name],
	                cl.[ParentTaxonId],
	                cl.[StartDate],
	                cl.[EndDate],
	                cl.[RegisterDate],
	                cl.[EditDate],
	                cl.[OccurrenceXCoord],
	                cl.[OccurrenceYCoord],
	                cl.[OccurrenceRange],
	                cl.[ProjectId],
	                p.FirstName,
	                p.LastName,
	                CASE 
		                WHEN ss.MinSiteId = ss.MaxSIteId THEN ss.MinSiteId 
		                ELSE NULL
	                END AS SiteId
                  FROM 
	                [Checklist] cl
	                INNER JOIN [User] u ON cl.ControlingUserId = u.Id
	                INNER JOIN [Person] p ON u.PersonId = p.Id
	                LEFT JOIN (
		                SELECT 
			                [ChecklistId],
			                Min(SiteId) AS MinSiteId,
			                MAX(SiteId) AS MaxSIteId
		                FROM 
			                [Sighting]
		                GROUP BY ChecklistId
	                ) AS ss ON cl.Id = ss.ChecklistId
                    WHERE 
                        cl.Id BETWEEN @StartId AND @EndId";

                return await QueryAsync<ChecklistEntity>(query, new { StartId = startId, EndId = startId + maxRows - 1 });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting checklists");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(int minId, int maxId)> GetIdSpanAsync()
        {
            try
            {
                string query = $@"
                SELECT 
                    MIN(cl.Id) AS minId,
                    MAX(cl.Id) AS maxId
		        FROM 
		            Checklist cl";

                return (await QueryAsync<(int minId, int maxId)>(query, null)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

                throw;
            }
        }
    }
}