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
        public static SightingAggregate ToAggregate(this SightingEntity entity,
            IDictionary<int, MetadataAggregate> activities,
            IDictionary<int, MetadataAggregate> genders,
            IDictionary<int, MetadataAggregate> stages,
            IDictionary<int, MetadataAggregate> units,
            IDictionary<int, TaxonAggregate> taxa,
            IDictionary<int, SiteAggregate> sites,
            IDictionary<int, List<ProjectAggregate>> projects)
        {
            return new SightingAggregate
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
        public static IEnumerable<SightingAggregate> ToAggregates(this IEnumerable<SightingEntity> entities,
            IDictionary<int, MetadataAggregate> activities,
            IDictionary<int, MetadataAggregate> genders,
            IDictionary<int, MetadataAggregate> stages,
            IDictionary<int, MetadataAggregate> units,
            IDictionary<int, TaxonAggregate> taxa,
            IDictionary<int, SiteAggregate> sites,
            IDictionary<int, List<ProjectAggregate>> projects)
        {
            return entities.Select(e => e.ToAggregate(activities, genders, stages, units, taxa, sites, projects));
        }

        /// <summary>
        /// Cast meta data entity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static MetadataAggregate ToAggregate(this MetadataEntity entity)
        {
            return new MetadataAggregate
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
        public static IEnumerable<MetadataAggregate> ToAggregates(this IEnumerable<MetadataEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        public static ProjectAggregate ToAggregate(this ProjectEntity entity)
        {
            return new ProjectAggregate()
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
        public static IEnumerable<ProjectAggregate> ToAggregates(this IEnumerable<ProjectEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        /// <summary>
        /// Cast site entity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static SiteAggregate ToAggregate(this SiteEntity entity)
        {
            return new SiteAggregate
            {
                County = entity.County,
                Id = entity.Id,
                Municipality = entity.Municipality,
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
        public static IEnumerable<SiteAggregate> ToAggregates(this IEnumerable<SiteEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        public static TaxonAggregate ToAggregate(this TaxonEntity entity)
        {
            return new TaxonAggregate
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
        public static IEnumerable<TaxonAggregate> ToAggregates(this IEnumerable<TaxonEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }
    }
}
