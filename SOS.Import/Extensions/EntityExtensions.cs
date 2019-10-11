using System.Collections.Generic;
using System.Linq;
using SOS.Import.Entities;
using SOS.Import.Enums;
using SOS.Import.Models.Aggregates.Artportalen;
using SOS.Import.Models.Shared;


namespace SOS.Import.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SightingExtensions
    {
        /// <summary>
        /// Cast area entity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Area ToAggregate(this AreaEntity entity)
        {
            return new Area((AreaType)entity.AreaDatasetId)
            {
                Id = entity.Id,
                Geometry = entity.Polygon.ToGeometry().Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84).ToGeoJsonGeometry(),
                Name = entity.Name
            };
        }

        /// <summary>
        /// Cast multiple area entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<Area> ToAggregates(this IEnumerable<AreaEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        /// <summary>
        /// Cast sighting entity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="activities"></param>
        /// <param name="genders"></param>
        /// <param name="stages"></param>
        /// <param name="units"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static APSightingVerbatim ToAggregate(this SightingEntity entity,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Site> sites,
            IDictionary<int, List<Project>> projects)
        {
            return new APSightingVerbatim
            {
                Activity = entity.ActivityId.HasValue && activities.ContainsKey(entity.ActivityId.Value) ? activities[entity.ActivityId.Value] : null,
                EndDate = entity.EndDate,
                EndTime = entity.EndTime,
                Gender = entity.GenderId.HasValue && genders.ContainsKey(entity.GenderId.Value)  ? genders[entity.GenderId.Value] : null,
                HiddenByProvider = entity.HiddenByProvider,
                Id = entity.Id,
                Length = entity.Length,
                NotPresent = entity.NotPresent,
                NotRecovered = entity.NotRecovered,
                Projects = projects.ContainsKey(entity.Id) ? projects[entity.Id] : null,
                ProtectedBySystem = entity.ProtectedBySystem,
                Quantity = entity.Quantity,
                Site = entity.SiteId.HasValue && sites.ContainsKey(entity.SiteId.Value) ? sites[entity.SiteId.Value] : null,
                Stage = entity.StageId.HasValue && stages.ContainsKey(entity.StageId.Value) ? stages[entity.StageId.Value] : null,
                StartDate = entity.StartDate,
                StartTime = entity.StartTime,
                TaxonId = entity.TaxonId,
                Unit = entity.UnitId.HasValue && units.ContainsKey(entity.UnitId.Value) ? units[entity.UnitId.Value] : null,
                Unspontaneous = entity.Unspontaneous,
                UnsureDetermination = entity.UnsureDetermination,
                Weight = entity.Weight
            };
        }

        /// <summary>
        ///  Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="activities"></param>
        /// <param name="genders"></param>
        /// <param name="stages"></param>
        /// <param name="units"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static IEnumerable<APSightingVerbatim> ToAggregates(this IEnumerable<SightingEntity> entities,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Site> sites,
            IDictionary<int, List<Project>> projects)
        {
            return entities.Select(e => e.ToAggregate(activities, genders, stages, units, sites, projects));
        }

        /// <summary>
        /// Cast meta data entity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Metadata ToAggregate(this MetadataEntity entity)
        {
            return new Metadata
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<Metadata> ToAggregates(this IEnumerable<MetadataEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        public static Project ToAggregate(this ProjectEntity entity)
        {
            return new Project()
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        /// <summary>
        /// Cast multiple projects entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<Project> ToAggregates(this IEnumerable<ProjectEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        /// <summary>
        /// Cast site entity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Site ToAggregate(this SiteEntity entity)
        {
            return new Site
            {
                County = entity.CountyId.HasValue ? new Metadata{ Id = entity.CountyId.Value, Name = entity.CountyName } : null,
                CountryPart = entity.CountryPartId.HasValue ? new Metadata { Id = entity.CountryPartId.Value, Name = entity.CountryPartName } : null,
                Id = entity.Id,
                Municipality = entity.MunicipalityId.HasValue ? new Metadata { Id = entity.MunicipalityId.Value, Name = entity.MunicipalityName } : null,
                Province = entity.ProvinceId.HasValue ? new Metadata { Id = entity.ProvinceId.Value, Name = entity.ProvinceName } : null,
                Name = entity.Name,
                XCoord = entity.XCoord,
                YCoord = entity.YCoord
            };
        }

        /// <summary>
        /// Cast multiple sites entities to models 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<Site> ToAggregates(this IEnumerable<SiteEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }
    }
}
