using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Import.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Project repository
    /// </summary>
    public class ProjectRepository : BaseRepository<ProjectRepository>, IProjectRepository
    {
        private string SelectSql => @"
                SELECT 
	                p.Id,
                    p.IsPublic,
                    p.IsHideall,
	                p.ProjectName AS Name,
                    p.ProjectDescription AS Description,
                    p.StartDate,
                    p.EndDate,
	                ten.Value AS Category,
	                t.Value AS CategorySwedish,
                    CONCAT('https://www.artportalen.se/Project/View/',p.Id) AS ProjectURL,
	                sm.Name AS SurveyMethod,
                    sm.Url AS SurveyMethodUrl,
	                CASE 
		                WHEN o.Id IS NOT NULL THEN o.Name
		                WHEN pn.Id IS NOT NULL THEN pn.FirstName + ' ' + pn.LastName 
	                END AS Owner
                FROM 
	                Project p 
	                LEFT JOIN ProjectCategory pc ON p.ProjectCategoryId = pc.Id
                    LEFT JOIN [Resource] cr ON pc.Name = cr.Label
                    LEFT JOIN Translation t ON cr.Id = t.ResourceId AND t.GlobalizationCultureId = 175
	                LEFT JOIN [Resource] cren ON pc.Name = cren.Label
                    LEFT JOIN Translation ten ON cren.Id = ten.ResourceId AND ten.GlobalizationCultureId = 49
	                LEFT JOIN SurveyMethod sm ON p.SurveyMethodId = sm.Id 
	                LEFT JOIN Organization o ON p.ControlingOrganisationId = o.Id
	                LEFT JOIN [User] u ON p.ControlingUserId = u.Id
	                LEFT JOIN Person pn ON u.PersonId = pn.Id";


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public ProjectRepository(IArtportalenDataService artportalenDataService, ILogger<ProjectRepository> logger) :
            base(artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProjectEntity>> GetProjectsAsync(bool live = false)
        {
            try
            {
                return await QueryAsync<ProjectEntity>(SelectSql, null, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting projects");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ProjectEntity> GetProjectAsync(int projectId, bool live = false)
        {
            try
            {
                var query = $@"{SelectSql} WHERE p.Id = @ProjectId";

                return (await QueryAsync<ProjectEntity>(query, new { ProjectId = projectId }, live)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error getting project: {projectId}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProjectParameterEntity>> GetSightingProjectParametersAsync(IEnumerable<int> sightingIds, bool live = false)
        {
            try
            {
                if (!sightingIds?.Any() ?? true)
                {
                    return null;
                }

                var query = $@"
                SELECT 
                    ss.SightingId AS SightingId, 
	                p.Id AS ProjectId, 
	                pp.Id AS ProjectParameterId, 
	                pp.Name, 
	                pp.Description, 
	                pp.Unit, 
	                ppv.Value, 
	                CASE 
		                WHEN pp.ProjectParameterTypeId = 3 
		                THEN 'double' 
		                ELSE 'string' 
                    END AS DataType               
	            FROM 
					SearchableSightings ss WITH (NOLOCK) 
                    INNER JOIN @tvp tvp ON ss.SightingId = tvp.Id 
	                INNER JOIN SightingState st ON ss.SightingId = st.SightingId 
		                AND st.SightingStateTypeId = 30 
		                AND st.IsActive = 1 
	                INNER JOIN Taxon t ON ss.TaxonId = t.Id 
	                INNER JOIN ProjectParameterValue ppv ON ss.SightingId = ppv.SightingId 
	                INNER JOIN ProjectParameter pp ON  pp.Id = ppv.ProjectParameterId 
	                INNER JOIN Project p ON p.Id = pp.ProjectId 
	            WHERE 
                    (ss.HiddenByProvider IS NULL OR ss.HiddenByProvider < getDate()) 
		            AND (ss.SightingTypeId = 0 OR ss.SightingTypeId = 3) 
		            AND ss.ValidationStatusId != 50 
		            AND t.ProtectionLevelId = 1  
		            AND ISNULL(ppv.Value, '') <> '' 
		            AND pp.IsDeleted = 0 
		            AND p.IsHideAll = 0";

                return await QueryAsync<ProjectParameterEntity>(
                    query, 
                    new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting project parameters");
                return null;
            }
        }
    }
}