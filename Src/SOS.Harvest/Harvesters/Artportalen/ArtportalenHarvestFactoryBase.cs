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
        private readonly ISiteRepository _siteRepository;
        protected readonly ConcurrentDictionary<int, Site> Sites;
        private readonly IAreaHelper _areaHelper;

        #region Site
        /// <summary>
        /// Try to add missing sites from live data
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        protected async Task AddMissingSitesAsync(IEnumerable<int>? siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return;
            }

            var siteEntities = await _siteRepository.GetByIdsAsync(siteIds, IncrementalMode);
            var siteAreas = await _siteRepository.GetSitesAreas(siteIds, IncrementalMode);
            var sitesGeometry = await _siteRepository.GetSitesGeometry(siteIds, IncrementalMode); // It's faster to get geometries in separate query than join it in site query

            var sites = await CastSiteEntitiesToVerbatimAsync(siteEntities?.ToArray(), siteAreas, sitesGeometry);

            if (sites?.Any() ?? false)
            {
                foreach (var site in sites)
                {
                    Sites.TryAdd(site.Id, site);
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
                XCoord = entity.XCoord,
                YCoord = entity.YCoord,
                VerbatimCoordinateSystem = CoordinateSys.WebMercator,
                ParentSiteId = entity.ParentSiteId
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
        #endregion Site

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteRepository"></param>
        /// <param name="areaHelper"></param>
        public ArtportalenHarvestFactoryBase(
            ISiteRepository siteRepository,
            IAreaHelper areaHelper) : base()
        {
            _siteRepository = siteRepository;
            _areaHelper = areaHelper;
            Sites = new ConcurrentDictionary<int, Site>();
        }

        public bool IncrementalMode { get; set; }
    }
}