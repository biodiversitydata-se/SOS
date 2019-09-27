using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Process.Entities;
using SOS.Process.Services.Interfaces;

namespace SOS.Process.Repositories.Source.SpeciesPortal
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
	                p.ProjectName AS Name
                FROM 
	                Project p";


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
