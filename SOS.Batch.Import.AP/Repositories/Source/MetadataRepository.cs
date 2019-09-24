using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP.Repositories.Source { 
    public class MetadataRepository : BaseRepository<MetadataRepository>, Interfaces.IMetadataRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public MetadataRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<MetadataRepository> logger) : base(speciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetActivitiesAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                a.Id, 
	                t.Value AS Name
                FROM 
	                Activity a 
	                INNER JOIN [Resource] r ON a.ResourceLabel = r.Label
	                INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting activities");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetGendersAsync()
        {
            try
            {
                
                    const string query = @"
                    SELECT 
	                    g.Id, 
	                    t.Value AS Name
                    FROM 
	                    Gender g 
	                    INNER JOIN [Resource] r ON g.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49";

                    return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting genders");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetStagesAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                s.Id, 
	                t.Value AS Name
                FROM 
	                Stage s
	                INNER JOIN [Resource] r ON s.ResourceLabel = r.Label
	                INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting stages");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetUnitsAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
	                    u.Id, 
	                    t.Value AS Name
                    FROM 
	                    Unit u 
	                    INNER JOIN [Resource] r ON u.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49";

                    return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting unite");
                return null;
            }
            
        }
    }
}
