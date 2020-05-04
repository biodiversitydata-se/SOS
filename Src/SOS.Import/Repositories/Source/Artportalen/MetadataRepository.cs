using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Repositories.Source.Artportalen
{ 
    public class MetadataRepository : BaseRepository<MetadataRepository>, Interfaces.IMetadataRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public MetadataRepository(IArtportalenDataService artportalenDataService, ILogger<MetadataRepository> logger) : base(artportalenDataService, logger)
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
	                t.Value AS Translation,
                    ac.Id AS CategoryId,
	                ct.Value AS CategoryName,
					gc.CultureCode
                FROM 
	                Activity a 
	                INNER JOIN [Resource] r ON a.ResourceLabel = r.Label
	                INNER JOIN Translation t ON r.Id = t.ResourceId
					INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
	                INNER JOIN ActivityCategory ac ON a.ActivityCategoryId = ac.Id
	                INNER JOIN [Resource] cr ON ac.ResourceLabel = cr.Label
	                INNER JOIN Translation ct ON cr.Id = ct.ResourceId AND ct.GlobalizationCultureId = t.GlobalizationCultureId 
				WHERE
					gc.Id IN (49, 175)
				ORDER BY
					a.Id,
					gc.CultureCode";

                return await QueryAsync<MetadataWithCategoryEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting activities");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetBiotopesAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
	                    b.Id, 
	                    t.Value AS Translation,
						gc.CultureCode
                    FROM 
	                    Biotope b 
	                    INNER JOIN [Resource] r ON b.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId 
						INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
					WHERE
						gc.Id IN (49, 175)
					ORDER BY
						b.Id,
						gc.CultureCode";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting biotopes");
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
	                    t.Value AS Translation,
						gc.CultureCode
                    FROM 
	                    Gender g 
	                    INNER JOIN [Resource] r ON g.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId 
						INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
					WHERE
						gc.Id IN (49, 175)
					ORDER BY
						g.Id,
						gc.CultureCode";

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
	                    Name AS Translation,
						'sv-SE' AS CultureCode
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
					t.Value AS Translation,
					gc.CultureCode
				FROM 
					Stage s
					INNER JOIN [Resource] r ON s.ResourceLabel = r.Label
					INNER JOIN Translation t ON r.Id = t.ResourceId 
					INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
				WHERE
					gc.Id IN (49, 175)
				ORDER BY
					s.Id,
					gc.CultureCode";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting stages");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetSubstratesAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
	                    s.Id, 
	                    t.Value AS Translation,
						gc.CultureCode
                    FROM 
	                    Substrate s 
	                    INNER JOIN [Resource] r ON s.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId 
						INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
					WHERE
						gc.Id IN (49, 175)
					ORDER BY
						s.Id,
						gc.CultureCode";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting substrates");
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
	                    t.Value AS Translation,
						gc.CultureCode
                    FROM 
	                    Unit u 
	                    INNER JOIN [Resource] r ON u.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId 
						INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
					WHERE
						gc.Id IN (49, 175)
					ORDER BY
						u.Id,
						gc.CultureCode";

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
	                    t.Value AS Translation,
						gc.CultureCode
                    FROM 
	                    ValidationStatus vs 
	                    INNER JOIN [Resource] r ON vs.ResourceLabel = r.Label
	                    INNER JOIN Translation t ON r.Id = t.ResourceId
						INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
					WHERE
						gc.Id IN (49, 175)
					ORDER BY
						vs.Id,
						gc.CultureCode";

                return await QueryAsync<MetadataEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting validation status");
                return null;
            }
        }
        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity>> GetAreaTypesAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
                        Id,
                        Name AS Translation,
                        'sv-SE' AS CultureCode 
                    FROM
                        AreaDataset 
                    WHERE 
                        CountryIsoCode = 752 AND Id IN @AreaTypes
                    UNION
                    SELECT 
                        Id,
                        Name AS Translation,
                        'en-GB' AS CultureCode 
                    FROM
                        AreaDataset 
                    WHERE 
                        CountryIsoCode = 752 AND Id IN @AreaTypes";

                var areaTypes = (int[])Enum.GetValues(typeof(AreaType));
                return await QueryAsync<MetadataEntity>(query, new { AreaTypes = areaTypes });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting area types status");
                return null;
            }
        }
    }
}
