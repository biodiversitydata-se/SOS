using System.Collections.Concurrent;
using System.Collections.Generic;
using SOS.Import.Entities.Artportalen;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Containers.Interfaces
{
    /// <summary>
    /// Artportalen metadata container
    /// </summary>
    public interface IArtportalenMetadataContainer
    {
        bool IsInitialized { get; }

        ConcurrentDictionary<int, MetadataWithCategory> Activities { get; }
        ConcurrentDictionary<int, Metadata> Biotopes { get; }
        ConcurrentDictionary<int, Metadata> DeterminationMethods { get; }
        ConcurrentDictionary<int, Metadata> DiscoveryMethods { get; }
        ConcurrentDictionary<int, Metadata> Genders { get; }
        ConcurrentDictionary<int, Organization> OrganizationById { get; }
        ConcurrentDictionary<int, Metadata> Organizations { get; }
        ConcurrentDictionary<int, Person> PersonByUserId { get; }
        ConcurrentDictionary<int, Project> Projects { get; }
        ConcurrentDictionary<int, Metadata> Stages { get; }
        ConcurrentDictionary<int, Metadata> Substrates { get; }
        ConcurrentDictionary<int, Metadata> Units { get; }
        ConcurrentDictionary<int, Metadata> ValidationStatus { get; }

        /// <summary>
        /// Initialize meta data
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="biotopes"></param>
        /// <param name="determinationMethods"></param>
        /// <param name="discoveryMethods"></param>
        /// <param name="genders"></param>
        /// <param name="organizationById"></param>
        /// <param name="organizations"></param>
        /// <param name="personByUserId"></param>
        /// <param name="projectEntities"></param>
        /// <param name="stages"></param>
        /// <param name="substrates"></param>
        /// <param name="units"></param>
        /// <param name="validationStatus"></param>
        void Initialize(
            IEnumerable<MetadataWithCategoryEntity> activities,
            IEnumerable<MetadataEntity> biotopes,
            IEnumerable<MetadataEntity> determinationMethods,
            IEnumerable<MetadataEntity> discoveryMethods,
            IEnumerable<MetadataEntity> genders,
            IEnumerable<OrganizationEntity> organizationById,
            IEnumerable<MetadataEntity> organizations,
            IEnumerable<PersonEntity> personByUserId,
            IEnumerable<ProjectEntity> projectEntities,
            IEnumerable<MetadataEntity> stages,
            IEnumerable<MetadataEntity> substrates,
            IEnumerable<MetadataEntity> units,
            IEnumerable<MetadataEntity> validationStatus);
    }
}
