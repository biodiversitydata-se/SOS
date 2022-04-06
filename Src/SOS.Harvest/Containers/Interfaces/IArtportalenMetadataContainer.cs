﻿using System.Collections.Concurrent;
using SOS.Harvest.Entities.Artportalen;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Containers.Interfaces
{
    /// <summary>
    /// Artportalen metadata container
    /// </summary>
    public interface IArtportalenMetadataContainer
    {
        bool IsInitialized { get; }
        /// <summary>
        /// Add a project
        /// </summary>
        /// <param name="entities"></param>
        void AddProject(ProjectEntity project);

        ConcurrentDictionary<int, MetadataWithCategory> Activities { get; }
        ConcurrentDictionary<int, Metadata> Biotopes { get; }
        ConcurrentDictionary<int, Metadata> DeterminationMethods { get; }
        ConcurrentDictionary<int, Metadata> DiscoveryMethods { get; }
        ConcurrentDictionary<int, Metadata> Genders { get; }
        ConcurrentDictionary<int, Metadata> Organizations { get; }
        ConcurrentDictionary<int, Person> PersonByUserId { get; }
        ConcurrentDictionary<int, Project> Projects { get; }
        ConcurrentDictionary<int, Metadata> Stages { get; }
        ConcurrentDictionary<int, Metadata> Substrates { get; }
        ConcurrentDictionary<int, int?> TaxonSpeciesGroups { get; }
        ConcurrentDictionary<int, Metadata> Units { get; }
        ConcurrentDictionary<int, Metadata> ValidationStatus { get; }

        /// <summary>
        ///  Initialize static meta data 
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="biotopes"></param>
        /// <param name="determinationMethods"></param>
        /// <param name="discoveryMethods"></param>
        /// <param name="genders"></param>
        /// <param name="organizations"></param>
        /// <param name="stages"></param>
        /// <param name="substrates"></param>
        /// <param name="taxa"></param>
        /// <param name="units"></param>
        /// <param name="validationStatus"></param>
        void InitializeStatic(
            IEnumerable<MetadataWithCategoryEntity> activities,
            IEnumerable<MetadataEntity> biotopes,
            IEnumerable<MetadataEntity> determinationMethods,
            IEnumerable<MetadataEntity> discoveryMethods,
            IEnumerable<MetadataEntity> genders,
            IEnumerable<MetadataEntity> organizations,
            IEnumerable<MetadataEntity> stages,
            IEnumerable<MetadataEntity> substrates,
            IEnumerable<TaxonEntity> taxa,
            IEnumerable<MetadataEntity> units,
            IEnumerable<MetadataEntity> validationStatus);

        /// <summary>
        ///  Initialize dynamic meta data
        /// </summary>
        /// <param name="personByUserId"></param>
        /// <param name="projectEntities"></param>
        void InitializeDynamic(
            IEnumerable<PersonEntity> personByUserId,
            IEnumerable<ProjectEntity> projectEntities);
    }
}