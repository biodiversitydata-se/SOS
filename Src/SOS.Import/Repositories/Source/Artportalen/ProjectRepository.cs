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
        public async Task<IEnumerable<ProjectEntity>> GetProjectsAsync()
        {
            try
            {
                const string query = @"
                SELECT 
	                p.Id,
                    p.IsPublic,
	                p.ProjectName AS Name,
                    p.ProjectDescription AS Description,
                    p.StartDate,
                    p.EndDate,
	                pc.Name AS Category,
	                sm.Name AS SurveyMethod,
                    sm.Url AS SurveyMethodUrl,
	                CASE 
		                WHEN o.Id IS NOT NULL THEN o.Name
		                WHEN pn.Id IS NOT NULL THEN pn.FirstName + ' ' + pn.LastName 
	                END AS Owner
                FROM 
	                Project p 
	                INNER JOIN ProjectCategory pc ON p.ProjectCategoryId = pc.Id
	                LEFT JOIN SurveyMethod sm ON p.SurveyMethodId = sm.Id 
	                LEFT JOIN Organization o ON p.ControlingOrganisationId = o.Id
	                LEFT JOIN [User] u ON p.ControlingUserId = u.Id
	                LEFT JOIN Person pn ON u.PersonId = pn.Id";


                return await QueryAsync<ProjectEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting projects");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProjectParameterEntity>> GetSightingProjectParametersAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                if (!sightingIds?.Any() ?? true)
                {
                    return null;
                }

                var query = $@"
                SELECT
                    SearchableSightings.SightingId							AS SightingId,
	                Project.Id												AS ProjectId,	                	                
	                ProjectParameter.Id										AS ProjectParameterId,
	                ProjectParameter.Name									AS Name,
	                ProjectParameter.Description							AS Description,
	                ProjectParameter.Unit									AS Unit,
	                ProjectParameterValue.Value								AS Value,
	                CASE
		                WHEN ProjectParameter.ProjectParameterTypeId = 3
		                THEN 'double'
		                ELSE 'string' END									AS DataType	                
	                FROM SearchableSightings SearchableSightings WITH (NOLOCK)	 
                    INNER JOIN @tvp t ON SearchableSightings.SightingId = t.Id 
	                INNER JOIN SightingState AS SightingState ON SightingState.SightingId = SearchableSightings.SightingId
		                AND SightingState.SightingStateTypeId = 30  -- A sighting that has been made public.
		                AND SightingState.IsActive = 1				-- if edited several records w/ IsActive = 0
	                INNER JOIN Taxon AS Taxon ON Taxon.Id = SearchableSightings.TaxonId
	                INNER JOIN ProjectParameterValue AS ProjectParameterValue ON SearchableSightings.SightingId = ProjectParameterValue.SightingId
	                INNER JOIN ProjectParameter AS ProjectParameter ON ProjectParameterValue.ProjectParameterId = ProjectParameter.Id
	                INNER JOIN Project AS Project ON Project.Id = ProjectParameter.ProjectId
	                WHERE
                        (SearchableSightings.HiddenByProvider IS NULL OR SearchableSightings.HiddenByProvider < getDate())
		                AND
		                (SearchableSightings.SightingTypeId = 0 OR SearchableSightings.SightingTypeId = 3)  
		                AND
		                SearchableSightings.ValidationStatusId != 50		                
		                AND
		                (Taxon.ProtectionLevelId = 1)		                
		                AND
		                ISNULL(ProjectParameterValue.Value, '') <> ''
		                AND
		                ProjectParameter.IsDeleted = 0
		                AND
		                Project.IsHideAll = 0";

                return await QueryAsync<ProjectParameterEntity>(
                    query, 
                    new { tvp = sightingIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting project parameters");
                return null;
            }
        }
    }
}