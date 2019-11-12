using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{ 
    public class MetadataRepository : BaseRepository<MetadataRepository>, Interfaces.IMetadataRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public MetadataRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<MetadataRepository> logger) : base(SpeciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataWithCategoryEntity>> GetActivitiesAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                a.Id, 
	                t.Value AS Name,
                    ac.Id AS CategoryId,
	                ct.Value AS CategoryName
                FROM 
	                Activity a 
	                INNER JOIN [Resource] r ON a.ResourceLabel = r.Label
	                INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49
	                INNER JOIN ActivityCategory ac ON a.ActivityCategoryId = ac.Id
	                INNER JOIN [Resource] cr ON ac.ResourceLabel = cr.Label
	                INNER JOIN Translation ct ON cr.Id = ct.ResourceId AND ct.GlobalizationCultureId = 49";

                return await QueryAsync<MetadataWithCategoryEntity>(query);
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
        public async Task<IEnumerable<MetadataEntity>> GetOrganizationsAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
	                    Id,
	                    Name
                    FROM 
                      Organization";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting organizations");
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

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetValidationStatusAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
	                    vs.Id, 
	                    t.Value AS Name
                    FROM 
	                    ValidationStatus vs 
	                    INNER JOIN [Resource] r ON vs.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting validation status");
                return null;
            }
        }
    }
}
