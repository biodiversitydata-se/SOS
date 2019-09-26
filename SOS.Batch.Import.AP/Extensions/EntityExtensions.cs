using System.Collections.Generic;
using System.Linq;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Models.Aggregates;

namespace SOS.Batch.Import.AP.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SightingExtensions
    {
        /// <summary>
        /// Cast sighting entity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="activities"></param>
        /// <param name="genders"></param>
        /// <param name="stages"></param>
        /// <param name="units"></param>
        /// <param name="taxa"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static APSightingVerbatim ToAggregate(this SightingEntity entity,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Taxon> taxa,
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
                Taxon = entity.TaxonId.HasValue && taxa.ContainsKey(entity.TaxonId.Value) ? taxa[entity.TaxonId.Value] : null,
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
        /// <param name="taxa"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static IEnumerable<APSightingVerbatim> ToAggregates(this IEnumerable<SightingEntity> entities,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Taxon> taxa,
            IDictionary<int, Site> sites,
            IDictionary<int, List<Project>> projects)
        {
            return entities.Select(e => e.ToAggregate(activities, genders, stages, units, taxa, sites, projects));
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

        public static Taxon ToAggregate(this TaxonEntity entity)
        {
            return new Taxon
            {
                Category = entity.Category,
                Id = entity.Id,
                ScientificName = entity.ScientificName,
                SwedishName = entity.SwedishName
            };
        }

        /// <summary>
        /// Cast multiple taxa entities to aggregates 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<Taxon> ToAggregates(this IEnumerable<TaxonEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }
    }
}
