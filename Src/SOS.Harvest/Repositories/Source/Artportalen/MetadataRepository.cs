using Amazon.Auth.AccessControlPolicy;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Enums;
using System.Transactions;
using System;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class MetadataRepository : BaseRepository<MetadataRepository>, IMetadataRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public MetadataRepository(IArtportalenDataService artportalenDataService, ILogger<MetadataRepository> logger) :
            base(artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataWithCategoryEntity<int>>> GetActivitiesAsync()
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

                return await QueryAsync<MetadataWithCategoryEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting activities");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetBiotopesAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting biotopes");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetGendersAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting genders");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetOrganizationsAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting organizations");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetStagesAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting stages");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetSubstratesAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting substrates");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetUnitsAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting unite");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetValidationStatusAsync()
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

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting validation status");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetAreaTypesAsync()
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

                var areaTypes = (int[]) Enum.GetValues(typeof(AreaType));
                return await QueryAsync<MetadataEntity<int>>(query, new {AreaTypes = areaTypes});
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting area types status");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetDiscoveryMethodsAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
                        dm.Id, 
                        t.Value AS Translation,
	                    gc.CultureCode
                    FROM 
                        DiscoveryMethod dm 
                        INNER JOIN [Resource] r ON dm.ResourceLabel = r.Label
                        INNER JOIN Translation t ON r.Id = t.ResourceId 
	                    INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
                    WHERE
	                    gc.Id IN (49, 175)
                    ORDER BY
	                    dm.Id,
	                    gc.CultureCode";
                
                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting discovery methods");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MetadataEntity<int>>> GetDeterminationMethodsAsync()
        {
            try
            {
                const string query = @"
                    SELECT 
                        dm.Id, 
                        t.Value AS Translation,
	                    gc.CultureCode
                    FROM 
                        DeterminationMethod dm 
                        INNER JOIN [Resource] r ON dm.ResourceLabel = r.Label
                        INNER JOIN Translation t ON r.Id = t.ResourceId 
	                    INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
                    WHERE
	                    gc.Id IN (49, 175)
                    ORDER BY
	                    dm.Id,
	                    gc.CultureCode";

                return await QueryAsync<MetadataEntity<int>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting determination methods");
                throw;
            }
        }

        public async Task<IEnumerable<MetadataEntity<string>>> GetResourcesAsync(string prefix)
        {
            try
            {
                string query = @$"
                    SELECT
                        r.Label AS Id,
                        t.Value AS Translation,
                        gc.CultureCode
                    FROM
                        Resource r
                        INNER JOIN Translation t ON r.Id = t.ResourceId
                        INNER JOIN GlobalizationCulture gc ON t.GlobalizationCultureId = gc.Id
                    WHERE
                        r.Label LIKE '{prefix}%'
                        AND gc.Id IN (49, 175)
                    ORDER BY
	                    r.Label,
	                    gc.CultureCode";

                return await QueryAsync<MetadataEntity<string>>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting resources");
                throw;
            }
        }

        

        /// <inheritdoc />
        public async Task<DateTime?> GetLastBackupDateAsync()
        {
            try
            {
                string query = @$"
                    SELECT
	                    MAX(ISNULL(r.[restore_date], d.create_date))
                    FROM 
	                    master.sys.databases d
	                    LEFT JOIN msdb.dbo.[restorehistory] r ON d.[name] = r.destination_database_name
                    WHERE
	                    d.[name] = '{ DataService.BackUpDatabaseName }'
                    GROUP BY         
	                    r.destination_database_name";

                return (await QueryAsync<DateTime?>(query)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting latest backup date");
                throw;
            }
        }
    }
}