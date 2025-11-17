using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen;

/// <summary>
///     Project repository
/// </summary>
public class ProjectRepository : BaseRepository<ProjectRepository>, IProjectRepository
{
    private string SelectSql => @"
                SELECT 
                    p.ControlingOrganisationId,
                    p.ControlingUserId,
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
	                END AS Owner,
                    u.UserServiceUserId
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

    private async Task AddProjectMembers(IEnumerable<ProjectEntity> projects)
    {
        if (!projects?.Any() ?? true)
        {
            return;
        }

        var projectDictionary = projects!.ToDictionary(p => p.Id, p => p);
        var query = @"
                SELECT 
                    pm.ProjectId, u.UserServiceUserId
                FROM
                    ProjectMember pm
                    INNER JOIN [User] u ON pm.UserId = u.Id
                    INNER JOIN @tvp tvp ON pm.ProjectId = tvp.Id 
                WHERE
                    u.UserServiceUserId > 0";

        var projectMembers = await QueryAsync<ProjectMemberEntity>(
                query,
                new { tvp = projects!.Select(p => p.Id).ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

        if (!projectMembers?.Any() ?? true)
        {
            return;
        }

        foreach (var projectMember in projectMembers!)
        {
            if (projectDictionary.TryGetValue(projectMember.ProjectId, out var project))
            {
                project.MembersIds ??= new HashSet<int>();
                project.MembersIds!.Add(projectMember.UserServiceUserId);
            }
        }
    }

    private async Task AddProjectParameters(IEnumerable<ProjectEntity> projects)
    {
        if (!projects?.Any() ?? true)
        {
            return;
        }

        var projectDictionary = projects.ToDictionary(p => p.Id, p => p);
        var query = @"SELECT 
	                    pp.Id,
	                    pp.ProjectId,
	                    pp.Name,
	                    pp.Description,
	                    pp.Unit,
                        CASE pp.ProjectParameterTypeId
		                    WHEN 3 
		                    THEN 'double' 
		                    ELSE 'string' 
                        END AS DataType   
                    FROM 
	                    ProjectParameter pp
                        INNER JOIN @tvp tvp ON pp.ProjectId = tvp.Id 
                    WHERE
	                    pp.IsDeleted = 0";

        var projectParams = await QueryAsync<ProjectParameterProjectEntity>(
                query,
                new { tvp = projects!.Select(p => p.Id).ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

        if (!projectParams?.Any() ?? true)
        {
            return;
        }
       
        foreach(var projectParam in projectParams!)
        {
            if(projectDictionary.TryGetValue(projectParam.ProjectId, out var project))
            {
                if (!project.Parameters?.Any() ?? true)
                {
                    project.Parameters = new HashSet<ProjectParameterEntity>();
                }
                project.Parameters!.Add(projectParam);
            }
        }

    }

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
    public async Task<IEnumerable<ProjectEntity>?> GetProjectsAsync()
    {
        try
        {
            var projects = await QueryAsync<ProjectEntity>(SelectSql, null!);
            await AddProjectParameters(projects);
            await AddProjectMembers(projects);

            return projects;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting projects");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProjectEntity?> GetProjectAsync(int projectId)
    {
        try
        {
            var query = $@"{SelectSql} WHERE p.Id = @ProjectId";

            var projects = await QueryAsync<ProjectEntity>(query, new { ProjectId = projectId });
            await AddProjectParameters(projects);
            await AddProjectMembers(projects);

            return projects?.FirstOrDefault();
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error getting project: {projectId}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProjectParameterSightingEntity>?> GetSightingProjectParametersAsync(IEnumerable<int> sightingIds)
    {
        try
        {
            if (!sightingIds?.Any() ?? true)
            {
                return null;
            }

            var query = $@"
                SELECT 
                    pp.Id, 
                    ss.SightingId AS SightingId, 
	                p.Id AS ProjectId, 
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

            return await QueryAsync<ProjectParameterSightingEntity>(
                query,
                new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting project parameters");
            throw;
        }
    }
}