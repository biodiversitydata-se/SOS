using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    /// <summary>
	///     Dataset repository
	/// </summary>
	public class DatasetRepository : BaseRepository<DatasetRepository>, IDatasetRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public DatasetRepository(IArtportalenDataService artportalenDataService, ILogger<DatasetRepository> logger) : base(
            artportalenDataService, logger)
        {
        }

        public async Task<DatasetEntities> GetDatasetEntitiesAsync()
        {
            var datasetEntities = new DatasetEntities();
            datasetEntities.Datasets = await GetDatasetsAsync();
            datasetEntities.AccessRights= await GetAccessRightsAsync();
            datasetEntities.Organisations= await GetOrganisationsAsync();
            datasetEntities.Methodologies= await GetMethodologiesAsync();
            datasetEntities.ProgrammeAreas= await GetProgrammeAreasAsync();
            datasetEntities.Projects= await GetProjectsAsync();
            datasetEntities.ProjectTypes= await GetProjectTypesAsync();
            datasetEntities.Purposes = await GetPurposesAsync();
            datasetEntities.DatasetCreatorRelations = await GetDatasetCreatorAsync();
            datasetEntities.DatasetMethodologyRelations = await GetDatasetMethodologyAsync();
            datasetEntities.DatasetProjectRelations = await GetDatasetProjectAsync();

            return datasetEntities;
        }

        private async Task<IEnumerable<DatasetEntities.DS_DatasetEntity>> GetDatasetsAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Identifier]
                          ,[Metadatalanguage]
                          ,[Language]
                          ,[AccessRightsId]
                          ,[PurposeId]
                          ,[AssignerId]
                          ,[CreatorId]
                          ,[OwnerinstitutionId]
                          ,[PublisherId]
                          ,[DataStewardship]
                          ,[StartDate]
                          ,[EndDate]
                          ,[Description]
                          ,[Title]
                          ,[Spatial]
                          ,[ProgrammeAreaId]
                          ,[DescriptionAccessRights]
                    FROM [DS_Dataset]";

                return await QueryAsync<DatasetEntities.DS_DatasetEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting datasets");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_AccessRights>> GetAccessRightsAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Value]
                    FROM [DS_AccessRights]";

                return await QueryAsync<DatasetEntities.DS_AccessRights>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting AccessRights");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_Organisation>> GetOrganisationsAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Identifier]
                          ,[Code]
                    FROM [DS_Organisation]";

                return await QueryAsync<DatasetEntities.DS_Organisation>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Organisation");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_Methodology>> GetMethodologiesAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Description]
                          ,[Link]
                          ,[SpeciesList]                          
                    FROM [dbo].[DS_Methodology]";

                return await QueryAsync<DatasetEntities.DS_Methodology>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Methodologies");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_ProgrammeArea>> GetProgrammeAreasAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Value]
                    FROM [DS_ProgrammeArea]";

                return await QueryAsync<DatasetEntities.DS_ProgrammeArea>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting ProgrammeArea");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_Project>> GetProjectsAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                            ,[ProjectId]
                            ,[ProjectCode]
                            ,[ProjectTypeId]
                            ,[ApProjectId]
                    FROM [DS_Project]";

                return await QueryAsync<DatasetEntities.DS_Project>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Projects");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_ProjectType>> GetProjectTypesAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Value]
                    FROM [DS_ProjectType]";

                return await QueryAsync<DatasetEntities.DS_ProjectType>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting ProjectType");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_Purpose>> GetPurposesAsync()
        {
            try
            {
                const string query = @"
                    SELECT [Id]
                          ,[Value]
                    FROM [DS_Purpose]";

                return await QueryAsync<DatasetEntities.DS_Purpose>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting Purpose");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_DatasetCreator>> GetDatasetCreatorAsync()
        {
            try
            {
                const string query = @"
                    SELECT [DatasetId]
                          ,[OrganisationId]
                    FROM [DS_DatasetCreator]";

                return await QueryAsync<DatasetEntities.DS_DatasetCreator>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting DatasetCreator");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_DatasetMethodology>> GetDatasetMethodologyAsync()
        {
            try
            {
                const string query = @"
                    SELECT [DatasetId]
                          ,[MethodologyId]
                    FROM [dbo].[DS_DatasetMethodology]";

                return await QueryAsync<DatasetEntities.DS_DatasetMethodology>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting DatasetMethodology");
                throw;
            }
        }

        private async Task<IEnumerable<DatasetEntities.DS_DatasetProject>> GetDatasetProjectAsync()
        {
            try
            {
                const string query = @"
                    SELECT [DatasetId]
                          ,[ProjectId]
                    FROM [dbo].[DS_DatasetProject]";

                return await QueryAsync<DatasetEntities.DS_DatasetProject>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting DatasetProject");
                throw;
            }
        }
    }
}