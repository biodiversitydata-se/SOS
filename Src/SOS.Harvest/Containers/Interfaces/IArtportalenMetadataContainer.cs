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
        ///  Initialize static meta data 
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Repository mode
        /// </summary>
        bool Live { set; }

        /// <summary>
        /// All organizations
        /// </summary>
        IDictionary<int, Metadata> Organizations { get; }

        /// <summary>
        /// All persons
        /// </summary>
        IDictionary<int, Person> PersonsByUserId { get; }

        /// <summary>
        /// Try to get a activity by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MetadataWithCategory TryGetActivity(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetBiotope(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetDeterminationMethod(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetDiscoveryMethod(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetGender(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetOrganization(int? id);

        /// <summary>
        /// Try get person by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Person> TryGetPersonByUserIdAsync(int? id);

        /// <summary>
        /// Try get project by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Project> TryGetProjectAsync(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetStage(int? id);

        /// <summary>
        /// Try get substrate by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetSubstrate(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int? TryGetTaxonSpeciesGroupId(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetUnit(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata TryGetValidationStatus(int? id);
    }
}