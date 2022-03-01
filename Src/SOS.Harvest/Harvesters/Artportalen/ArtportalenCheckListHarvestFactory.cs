﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Extensions;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Harvesters.Artportalen
{
    internal class ArtportalenCheckListHarvestFactory : ArtportalenHarvestFactoryBase, IHarvestFactory<CheckListEntity[], ArtportalenCheckListVerbatim>
    {
        private readonly IDictionary<int, Project> _projects;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ILogger<ArtportalenCheckListHarvester> _logger;

        /// <summary>
        /// Cast check list entity to model
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="taxonIds"></param>
        /// <param name="sightingData"></param>
        /// <returns></returns>
        private ArtportalenCheckListVerbatim CastEntityToVerbatim(CheckListEntity entity, IEnumerable<int> taxonIds, IEnumerable<(int sightingId, int taxonId)> sightingData)
        {
            try
            {
                if (entity == null)
                {
                    return null;
                }

                if (Sites.TryGetValue(entity.SiteId.HasValue ? entity.SiteId.Value : -1, out var site))
                {
                    // Try to set parent site name if empty
                    if (site?.ParentSiteId != null && string.IsNullOrEmpty(site.ParentSiteName))
                    {
                        if (Sites.TryGetValue(site.ParentSiteId.Value, out var parentSite))
                        {
                            site.ParentSiteName = parentSite.Name;
                        }
                    }
                }

                _projects.TryGetValue(entity.ProjectId ?? 0, out var project);

                var checkListVerbatim = new ArtportalenCheckListVerbatim
                {
                    ControlingUserId = entity.ControlingUserId,
                    ControllingUser = entity.ControllingUser,
                    EditDate = entity.EditDate,
                    EndDate = entity.EndDate,
                    Id = entity.Id,
                    Name = entity.Name,
                    OccurrenceRange = entity.OccurrenceRange,
                    OccurrenceXCoord = entity.OccurrenceXCoord,
                    OccurrenceYCoord = entity.OccurrenceYCoord,
                    ParentTaxonId = entity.ParentTaxonId,
                    Project = project,
                    RegisterDate = entity.RegisterDate,
                    SightingIds = sightingData?.Select(sd => sd.sightingId),
                    Site = site,
                    StartDate = entity.StartDate,
                    TaxonIds = taxonIds,
                    TaxonIdsFound = sightingData?.Select(sd => sd.taxonId).Distinct()
                };

                return checkListVerbatim;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to cast Artportalen check list entity with id:{entity.Id}");
                throw;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaHelper"></param>
        /// <param name="checkListRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="projectEntities"></param>
        /// <param name="logger"></param>
        public ArtportalenCheckListHarvestFactory(
            IAreaHelper areaHelper,
            ICheckListRepository checkListRepository,
            ISiteRepository siteRepository,
            ISightingRepository sightingRepository,
            IEnumerable<ProjectEntity> projectEntities,
            ILogger<ArtportalenCheckListHarvester> logger) : base(siteRepository, areaHelper)
        {
            _checkListRepository = checkListRepository ?? throw new ArgumentNullException(nameof(checkListRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _projects = projectEntities?.ToDictionary(pe => pe.Id, pe => pe.ToVerbatim()) ?? new Dictionary<int, Project>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IncrementalMode { get; set; }

        /// <inheritdoc />
        public async Task<IEnumerable<ArtportalenCheckListVerbatim>> CastEntitiesToVerbatimsAsync(CheckListEntity[] entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            var newSiteIds = new HashSet<int>();

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var siteId = entity.SiteId ?? 0;

                // Check for new sites since we already lopping the array 
                if (siteId == 0 || newSiteIds.Contains(siteId) || Sites.ContainsKey(siteId))
                {
                    continue;
                }

                newSiteIds.Add(siteId);
            }

            await AddMissingSitesAsync(newSiteIds);

            _logger.LogDebug(
                "Start getting check lists metadata");

            var checkListIds = entities.Select(cl => cl.Id);
            var checkListsTaxonIds = await _checkListRepository.GetCheckListsTaxonIdsAsync(checkListIds);
            var chekListsSightingsData =
                await _sightingRepository.GetSightingsAndTaxonIdsForCheckListsAsync(checkListIds);

            _logger.LogDebug(
                "Finish getting check lists metadata");

            var verbatims = new List<ArtportalenCheckListVerbatim>();
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                checkListsTaxonIds.TryGetValue(entity.Id, out var taxonIds);
                chekListsSightingsData.TryGetValue(entity.Id, out var sightingData);
                verbatims.Add(CastEntityToVerbatim(entity, taxonIds, sightingData));
            }

            return verbatims;
        }

    }
}