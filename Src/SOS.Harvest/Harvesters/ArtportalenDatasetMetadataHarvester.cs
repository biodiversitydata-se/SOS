using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Harvest.Harvesters
{
    /// <summary>
    ///     Class for harvest projects.
    /// </summary>
    public class ArtportalenDatasetMetadataHarvester : IArtportalenDatasetMetadataHarvester
    {
        private readonly IDatasetRepository _artportalenDatasetRepository;
        private readonly IArtportalenDatasetMetadataRepository _artportalenDatasetMetadataRepository;        
        //private readonly ICacheManager _cacheManager;
        private readonly ILogger<ArtportalenDatasetMetadataHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenProjectRepository"></param>
        /// <param name="projectInfoRepository"></param>
        /// <param name="cacheManager"></param>
        /// <param name="logger"></param>
        public ArtportalenDatasetMetadataHarvester(
            IDatasetRepository artportalenDatasetRepository,
            IArtportalenDatasetMetadataRepository artportalenDatasetMetadataRepository,
            ILogger<ArtportalenDatasetMetadataHarvester> logger)
        {
            _artportalenDatasetRepository = artportalenDatasetRepository ?? throw new ArgumentNullException(nameof(artportalenDatasetRepository));
            _artportalenDatasetMetadataRepository = artportalenDatasetMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenDatasetMetadataRepository));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestDatasetsAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(ArtportalenDatasetMetadata), DateTime.Now);
            try
            {
                _logger.LogDebug("Start getting Artportalen datasets metadata");

                var datasetEntities = await _artportalenDatasetRepository.GetDatasetEntitiesAsync();
                if (datasetEntities == null)
                {
                    harvestInfo.Status = RunStatus.Failed;
                    return harvestInfo;
                }

                var datasets = CastToArtportalenDatasetVerbatims(datasetEntities);
                _logger.LogDebug("Finish getting Artportalen datasets metadata");

                if (await _artportalenDatasetMetadataRepository.DeleteCollectionAsync())
                {
                    if (await _artportalenDatasetMetadataRepository.AddCollectionAsync())
                    {
                        await _artportalenDatasetMetadataRepository.AddManyAsync(datasets);                        

                        // Update harvest info
                        harvestInfo.End = DateTime.Now;
                        harvestInfo.Status = RunStatus.Success;
                        harvestInfo.Count = datasets?.Count() ?? 0;

                        _logger.LogDebug("Adding Artportalen datasets metadata succeeded");
                        return harvestInfo;
                    }
                }

                _logger.LogDebug("Failed harvest of Artportalen datasets metadata");
                harvestInfo.Status = RunStatus.Failed;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of Artportalen datasets metadata");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        private IEnumerable<ArtportalenDatasetMetadata> CastToArtportalenDatasetVerbatims(DatasetEntities datasetEntities)
        {
            var accessRightsById = datasetEntities.AccessRights.Select(m => CastToAccessRights(m))
                .ToDictionary(m => m.Id, m => m);
            var organisationById = datasetEntities.Organisations.Select(m => CastToOrganisation(m))
                .ToDictionary(m => m.Id, m => m);
            var methodologyById = datasetEntities.Methodologies.Select(m => CastToMethodology(m))
                .ToDictionary(m => m.Id, m => m);
            var programmeAreaById = datasetEntities.ProgrammeAreas.Select(m => CastToProgrammeArea(m))
                .ToDictionary(m => m.Id, m => m);
            var projectTypeById = datasetEntities.ProjectTypes.Select(m => CastToProjectType(m))
                .ToDictionary(m => m.Id, m => m);
            var projectById = datasetEntities.Projects.Select(m => CastToProject(m, projectTypeById))
                .ToDictionary(m => m.Id, m => m);
            var purposeById = datasetEntities.Purposes.Select(m => CastToPurpose(m))
                .ToDictionary(m => m.Id, m => m);

            var datasets = datasetEntities.Datasets.Select(m => CastToDataset(m,
                organisationById,
                methodologyById,
                projectById,
                accessRightsById,
                purposeById,
                programmeAreaById,
                datasetEntities.DatasetCreatorRelations.Where(d => d.DatasetId == m.Id).Select(x => x.OrganisationId),
                datasetEntities.DatasetMethodologyRelations.Where(d => d.DatasetId == m.Id).Select(x => x.MethodologyId),
                datasetEntities.DatasetProjectRelations.Where(d => d.DatasetId == m.Id).Select(x => x.ProjectId)
            ));

            return datasets;
        }

        private ArtportalenDatasetMetadata CastToDataset(DatasetEntities.DS_DatasetEntity dsDataset,
            Dictionary<int, ArtportalenDatasetMetadata.Organisation> organisationById,
            Dictionary<int, ArtportalenDatasetMetadata.Methodology> methodologyById,
            Dictionary<int, ArtportalenDatasetMetadata.Project> projectById,
            Dictionary<int, ArtportalenDatasetMetadata.AccessRights> accessRightsById,
            Dictionary<int, ArtportalenDatasetMetadata.Purpose> purposeById,
            Dictionary<int, ArtportalenDatasetMetadata.ProgrammeArea> programmeAreaById,
            IEnumerable<int> creatorIds,
            IEnumerable<int> methodologyIds,
            IEnumerable<int> projectIds)
        {
            return new ArtportalenDatasetMetadata
            {
                Id = dsDataset.Id,
                Assigner = organisationById[dsDataset.AssignerId],
                OwnerInstitution = organisationById[dsDataset.OwnerinstitutionId],
                Publisher = organisationById[dsDataset.PublisherId],
                Creators = creatorIds.Select(m => organisationById[m]),
                Methodologies = methodologyIds.Select(m => methodologyById[m]),
                Projects = projectIds.Select(m => projectById[m]),
                Identifier = dsDataset.Identifier,
                Metadatalanguage = dsDataset.Metadatalanguage,
                Language = dsDataset.Language,
                DataStewardship = dsDataset.DataStewardship,
                StartDate = dsDataset.StartDate,
                EndDate = dsDataset.EndDate,
                Description = dsDataset.Description,
                Title = dsDataset.Title,
                Spatial = dsDataset.Spatial,
                DescriptionAccessRights = dsDataset.DescriptionAccessRights,        
                DatasetAccessRights = accessRightsById[dsDataset.AccessRightsId],
                DatasetPurpose = purposeById[dsDataset.PurposeId],
                DatasetProgrammeArea = programmeAreaById[dsDataset.ProgrammeAreaId]
            };
        }

        private ArtportalenDatasetMetadata.AccessRights CastToAccessRights(DatasetEntities.DS_AccessRights dsAccessRights)
        {
            return new ArtportalenDatasetMetadata.AccessRights
            {
                Id = dsAccessRights.Id,
                Value = dsAccessRights.Value
            };
        }

        private ArtportalenDatasetMetadata.Organisation CastToOrganisation(DatasetEntities.DS_Organisation dsOrganisation)
        {
            return new ArtportalenDatasetMetadata.Organisation
            {
                Id = dsOrganisation.Id,
                Code = dsOrganisation.Code,
                Identifier = dsOrganisation.Identifier
            };
        }

        private ArtportalenDatasetMetadata.Methodology CastToMethodology(DatasetEntities.DS_Methodology dsMethodology)
        {
            return new ArtportalenDatasetMetadata.Methodology
            {
                Id = dsMethodology.Id,
                Description = dsMethodology.Description,
                Link = dsMethodology.Link,
                Name = dsMethodology.Name
            };
        }

        private ArtportalenDatasetMetadata.ProgrammeArea CastToProgrammeArea(DatasetEntities.DS_ProgrammeArea dsProgrammeArea)
        {
            return new ArtportalenDatasetMetadata.ProgrammeArea
            {
                Id = dsProgrammeArea.Id,
                Value = dsProgrammeArea.Value
            };
        }

        private ArtportalenDatasetMetadata.ProjectType CastToProjectType(DatasetEntities.DS_ProjectType dsProjectType)
        {
            return new ArtportalenDatasetMetadata.ProjectType
            {
                Id = dsProjectType.Id,
                Value = dsProjectType.Value
            };
        }

        private ArtportalenDatasetMetadata.Project CastToProject(DatasetEntities.DS_Project dsProject, 
            Dictionary<int, ArtportalenDatasetMetadata.ProjectType> projectTypeById)
        {
            return new ArtportalenDatasetMetadata.Project
            {
                Id = dsProject.Id,
                ApProjectId = dsProject.ApProjectId,
                ProjectCode = dsProject.ProjectCode,
                ProjectId = dsProject.ProjectId,
                ProjectType = projectTypeById[dsProject.ProjectTypeId]
            };
        }

        private ArtportalenDatasetMetadata.Purpose CastToPurpose(DatasetEntities.DS_Purpose dsPurpose)
        {
            return new ArtportalenDatasetMetadata.Purpose
            {
                Id = dsPurpose.Id,
                Value = dsPurpose.Value
            };
        }
    }
}