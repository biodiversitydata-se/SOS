using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP.Repositories.Source { 
    public class SightingRepository : BaseRepository<SightingRepository>, Interfaces.ISightingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public SightingRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<SightingRepository> logger) : base(speciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = @"
                SELECT DISTINCT
                    s.Id, 
                    s.TaxonId,
                    s.StartDate,
                    s.StartTime,
                    s.EndDate,
                    s.EndTime,
                    s.SiteId,
                    s.Quantity,
                    s.[Length],
                    s.[Weight],
                    s.UnitId,
                    s.StageId,
                    s.ActivityId,
                    s.GenderId,
                    s.UnsureDetermination,
                    s.Unspontaneous,
                    s.NotRecovered,
                    s.NotPresent,
                    s.ProtectedBySystem,
                    s.HiddenByProvider
                FROM
                    SearchableSightings s WITH(NOLOCK)
                    INNER JOIN SightingState ss ON s.SightingId = ss.SightingId
                WHERE
                    s.Id BETWEEN @StartId AND @EndId
                    AND s.TaxonId IS NOT NULL
                    AND s.SightingTypeId IN(0, 3) 
                    AND s.HiddenByProvider IS NULL
                    AND s.ValidationStatusId NOT IN(50)
                    AND s.SightingTypeSearchGroupId & 33 > 0
                    AND ss.IsActive = 1
                    AND ss.SightingStateTypeId = 30--Published
                    AND(ss.EndDate IS NULL OR ss.EndDate > GETDATE())";

                return await QueryAsync<SightingEntity>(query, new { StartId = startId, EndId = startId + maxRows -1 });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        public async Task<Tuple<int, int>> GetIdSpanAsync()
        {
            try
            {
                const string query = @"
                SELECT 
                    MIN(Id) AS Item1,
                    MAX(Id) AS Item2
		        FROM 
		            SearchableSightings s";

               return (await QueryAsync<Tuple<int, int>>(query)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

                return null;
            }
        }

        public async Task<IEnumerable<Tuple<int, int>>> GetProjectIdsAsync()
        {
            try
            {
                const string query = @"
                SELECT 
                    ps.SightingId AS Item1,
	                ps.ProjectId AS Item2
                FROM 
	                ProjectSighting ps WITH(NOLOCK)";

                return await QueryAsync<Tuple<int, int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting/project connections");

                return null;
            }
        }
    }
}
