using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP.Repositories.Source {
    /// <summary>
    /// Site repository
    /// </summary>
    public class SiteRepository : BaseRepository<SiteRepository>, Interfaces.ISiteRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public SiteRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<SiteRepository> logger) : base(speciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SiteEntity>> GetAsync()
        {
            try
            {
               
                const string query = @"
                SELECT 
	                s.Id,
	                s.Name,
	                s.XCoord,
	                s.YCoord,
	                am.Name AS Municipality,
	                am.ParentAreaName AS County
                FROM 
	                Site s 
	                INNER JOIN -- Bad data exists, some sites are connected to more than on Municipality
	                (
		                SELECT
			                sa.SiteId,
			                MAX(sa.AreasId) AS AreasId
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 1 
		                GROUP BY 
			                sa.SiteId
	                ) AS sam ON s.Id = sam.SiteId 
	                INNER JOIN Area am ON sam.AreasId = am.Id";

                return await QueryAsync<SiteEntity>(query);
            }
            catch(Exception e)
            {
                Logger.LogError(e, "Error getting sites");
                return null;
            }
            
        }
    }
}
