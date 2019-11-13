using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Project repository
    /// </summary>
    public class ProjectRepository : BaseRepository<ProjectRepository>, Interfaces.IProjectRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public ProjectRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<ProjectRepository> logger) : base(SpeciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProjectEntity>> GetAsync()
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
    }
}
