using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP.Repositories.Source {
    /// <summary>
    /// Project repository
    /// </summary>
    public class ProjectRepository : BaseRepository<ProjectRepository>, Interfaces.IProjectRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public ProjectRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<ProjectRepository> logger) : base(speciesPortalDataService, logger)
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
