using System.Collections.Concurrent;
using NetTopologySuite.Geometries;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Harvesters.Artportalen
{
    internal class ArtportalenHarvestFactoryBase : HarvestBaseFactory
    {
        private readonly IAreaHelper _areaHelper;
        private readonly ConcurrentDictionary<int, Site> _cachedSites;
        private readonly SemaphoreSlim _semaphore;
        private readonly ISiteRepository _siteRepository;

        #region Site
        /// <summary>
        /// Try to add missing sites from live data
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        private async Task AddMissingSitesAsync(IEnumerable<int>? siteIds)
        {
            var sites = await GetSitesAsync(siteIds);

            if (sites?.Any() ?? false)
            {
                foreach (var site in sites)
                {
                    _cachedSites.TryAdd(site.Id, site);
                }
            }
        }

        /// <summary>
        /// Cast multiple sites entities to models by continuously decreasing the siteEntities input list.
        ///     This saves about 500MB RAM when casting Artportalen sites (3 millions).
        /// </summary>
        /// <param name="siteEntities"></param>
        /// <param name="sitesAreas"></param>
        /// <param name="sitesGeometry"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Site>> CastSiteEntitiesToVerbatimAsync(IEnumerable<SiteEntity>? siteEntities, IDictionary<int, ICollection<AreaEntityBase>>? sitesAreas, IDictionary<int, string>? sitesGeometry)
        {
            var sites = new HashSet<Site>();

            if (!siteEntities?.Any() ?? true)
            {
                return sites;
            }

            // Make sure metadata are initialized
            sitesAreas ??= new Dictionary<int, ICollection<AreaEntityBase>>();
            sitesGeometry ??= new Dictionary<int, string>();

            foreach (var siteEntity in siteEntities)
            {
                sitesAreas.TryGetValue(siteEntity.Id, out var siteAreas);
                sitesGeometry.TryGetValue(siteEntity.Id, out var geometryWkt);

                var site = CastSiteEntityToVerbatim(siteEntity, siteAreas, geometryWkt);

                if (site != null)
                {
                    sites.Add(site);
                }
            }

            return sites;
        }

        /// <summary>
        /// Cast site itemEntity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="areas"></param>
        /// <param name="geometryWkt"></param>
        /// <returns></returns>
        private Site? CastSiteEntityToVerbatim(SiteEntity? entity, ICollection<AreaEntityBase>? areas, string? geometryWkt)
        {
            if (entity == null)
            {
                return null;
            }

            Point? wgs84Point = null;
            const int defaultAccuracy = 100;

            if (entity.XCoord > 0 && entity.YCoord > 0)
            {
                // We process point here since site is added to observation verbatim. One site can have multiple observations and by 
                // doing it here we only have to convert the point once
                var webMercatorPoint = new Point(entity.XCoord, entity.YCoord);
                wgs84Point = (Point)webMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
            }

            Geometry? siteGeometry = null;
            if (!string.IsNullOrEmpty(geometryWkt))
            {
                siteGeometry = geometryWkt.ToGeometry()
                    .Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84).TryMakeValid();
            }

            var accuracy = entity.Accuracy > 0 ? entity.Accuracy : defaultAccuracy; // If Artportalen site accuracy is <= 0, this is due to an old import. Set the accuracy to 100.
            var site = new Site
            {
                Accuracy = accuracy,
                ExternalId = entity.ExternalId,
                Id = entity.Id,
                PresentationNameParishRegion = entity.PresentationNameParishRegion,
                Point = wgs84Point?.ToGeoJson(),
                PointWithBuffer = (siteGeometry?.IsValid() ?? false ? siteGeometry : wgs84Point.ToCircle(accuracy))?.ToGeoJson(),
                Name = entity.Name,
                ParentSiteId = entity.ParentSiteId,
                ParentSiteName = entity.ParentSiteName,
                XCoord = entity.XCoord,
                YCoord = entity.YCoord,
                VerbatimCoordinateSystem = CoordinateSys.WebMercator,

            };

            if (!areas?.Any() ?? true)
            {
                return site;
            }

            foreach (var area in areas)
            {
                switch ((AreaType)area.AreaDatasetId)
                {
                    case AreaType.BirdValidationArea:
                        (site.BirdValidationAreaIds ??= new List<string>()).Add(area.FeatureId);
                        break;
                    case AreaType.County:
                        site.County = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                    case AreaType.Municipality:
                        site.Municipality = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                    case AreaType.Parish:
                        site.Parish = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                    case AreaType.Province:
                        site.Province = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                }
            }

            _areaHelper.AddAreaDataToSite(site);

            return site;
        }

        /// <summary>
        /// Get sites and related metadata and create site objects
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Site>> GetSitesAsync(IEnumerable<int>? siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return null;
            }

            var skip = 0;
            const int take = 10000;
            var siteIdBatch = siteIds.Take(take);
            var sites = new List<Site>();

            while (siteIdBatch.Any())
            {
                await _semaphore.WaitAsync();
                var siteBatch = await GetSiteChunkAsync(siteIdBatch);

                if (siteBatch?.Any() ?? false)
                {
                    sites.AddRange(siteBatch);
                }
               
                skip += take;
                siteIdBatch = siteIds.Skip(skip).Take(take);
            }

            return sites;
        }

        private async Task<IEnumerable<Site>> GetSiteChunkAsync(IEnumerable<int>? siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                var getSitesTask = _siteRepository.GetByIdsAsync(siteIds, Live);
                var getSitesAreasTask = _siteRepository.GetSitesAreas(siteIds, Live);
                var getSitesGeometriesTask = _siteRepository.GetSitesGeometry(siteIds, Live);

                var siteEntities = await getSitesTask;
                var siteAreas = await getSitesAreasTask;
                var sitesGeometry = await getSitesGeometriesTask; // It's faster to get geometries in separate query than join it in site query

                return await CastSiteEntitiesToVerbatimAsync(siteEntities?.ToArray(), siteAreas, sitesGeometry);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Get sites used in current batch
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        protected async Task<IDictionary<int, Site>> GetBatchSitesAsync(IEnumerable<int>? siteIds)
        {
            var sites = new Dictionary<int, Site>();
            
            if (!siteIds?.Any() ?? true)
            {
                return sites;
            }

            var missingSiteIds = new HashSet<int>();

            // Try to get sites from cache
            foreach (var siteId in siteIds)
            {
                if (_cachedSites.TryGetValue(siteId, out var site))
                {
                    sites.Add(siteId, site);
                    continue;
                }
                missingSiteIds.Add(siteId);
            }

            // Get sites that not exists in cache from db
            if (missingSiteIds.Any())
            {
                var missingSites = await GetSitesAsync(missingSiteIds);

                if (missingSites?.Any() ?? false)
                {
                    foreach (var site in missingSites)
                    {
                        sites.TryAdd(site.Id, site);
                    }
                }
            }

            return sites;
        }

        /// <summary>
        /// Check if site exists in cache
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        protected bool IsSiteLoaded(int siteId) => _cachedSites.ContainsKey(siteId);

        protected readonly bool Live;

        protected Site TryGetSite(int siteId) {
            _cachedSites.TryGetValue(siteId, out var site);

            return site;
        }
        #endregion Site

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="live"></param>
        /// <param name="noOfThreads"></param>
        public ArtportalenHarvestFactoryBase(
            ISiteRepository siteRepository,
            IAreaHelper areaHelper,
            bool live,
            int noOfThreads) : base()
        {
            _siteRepository = siteRepository;
            _areaHelper = areaHelper;
            _cachedSites = new ConcurrentDictionary<int, Site>();
            Live = live;
            _semaphore = new SemaphoreSlim(noOfThreads);
        }

        /// <summary>
        /// Cache sites used multiple times
        /// </summary>
        /// <returns></returns>
        public async Task CacheFreqventlyUsedSitesAsync()
        {
            // Should only be called once
            if (!_cachedSites.Any())
            {
                var siteIds = await _siteRepository.GetFreqventlyUsedIdsAsync(Live);
                await AddMissingSitesAsync(siteIds);
            }
        }
    }
}