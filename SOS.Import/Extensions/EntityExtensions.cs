using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Import.Entities;
using SOS.Import.Models;
using SOS.Import.Models.Aggregates;

namespace SOS.Import.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SightingExtensions
    {
        /// <summary>
        /// Cast sighting itemEntity to model 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="activities"></param>
        /// <param name="genders"></param>
        /// <param name="stages"></param>
        /// <param name="units"></param>
        /// <param name="sites"></param>
        /// <param name="projects"></param>
        /// <param name="personSightings"></param>
        /// <returns></returns>
        public static APSightingVerbatim ToAggregate(this SightingEntity entity,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Site> sites,
            IDictionary<int, List<Project>> projects, 
            IDictionary<int, PersonSighting> personSightings)
        {
            var observation = new APSightingVerbatim
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

            if (personSightings.TryGetValue(entity.Id, out PersonSighting personSighting))
            {
                observation.VerifiedBy = personSighting.VerifiedBy;
                observation.Observers = personSighting.Observers;
                observation.ReportedBy = personSighting.ReportedBy;
                observation.SpeciesCollection = personSighting.SpeciesCollection;
            }

            return observation;
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
        /// <param name="personSightings"></param>
        /// <returns></returns>
        public static IEnumerable<APSightingVerbatim> ToAggregates(this IEnumerable<SightingEntity> entities,
            IDictionary<int, Metadata> activities,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> units,
            IDictionary<int, Site> sites,
            IDictionary<int, List<Project>> projects, 
            IDictionary<int, PersonSighting> personSightings)
        {
            return entities.Select(e => e.ToAggregate(activities, genders, stages, units, sites, projects, personSightings));
        }

        /// <summary>
        /// Cast meta data itemEntity to model 
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

        public static IEnumerable<Person> ToAggregates(this IEnumerable<PersonEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        public static Person ToAggregate(this PersonEntity entity)
        {
            return new Person()
            {
                Id = entity.Id,
                UserId = entity.UserId,
                FirstName = entity.FirstName,
                LastName = entity.LastName
            };
        }

        public static IEnumerable<SpeciesCollectionItem> ToAggregates(this IEnumerable<SpeciesCollectionItemEntity> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        public static SpeciesCollectionItem ToAggregate(this SpeciesCollectionItemEntity itemEntity)
        {
            return new SpeciesCollectionItem()
            {
                SightingId = itemEntity.SightingId,
                CollectorId = itemEntity.CollectorId,
                OrganizationId = itemEntity.OrganizationId,
                DeterminerText = itemEntity.DeterminerText,
                DeterminerYear = itemEntity.DeterminerYear,
                Description = itemEntity.Description,
                ConfirmatorText = itemEntity.ConfirmatorText,
                ConfirmatorYear = itemEntity.ConfirmatorYear
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
        /// Cast site itemEntity to aggregate
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
