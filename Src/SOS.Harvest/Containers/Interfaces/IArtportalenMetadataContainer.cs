
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
        IDictionary<int, Metadata<int>>? Organizations { get; }

        /// <summary>
        /// All persons
        /// </summary>
        IDictionary<int, Person>? PersonsByUserId { get; }

        /// <summary>
        /// Try to get a activity by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MetadataWithCategory<int>? TryGetActivity(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetBiotope(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetDeterminationMethod(int? id);

        /// <summary>
        /// Try get diary entry
        /// </summary>
        /// <param name="projectIds"></param>
        /// <param name="startDate"></param>
        /// <param name="startTime"></param>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <param name="controlingOrganisationId"></param>
        /// <returns></returns>
        DiaryEntry? TryGetDiaryEntry(IEnumerable<int> projectIds, DateTime? startDate, TimeSpan? startTime, int userId, int? siteId, int? controlingOrganisationId);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetDiscoveryMethod(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetGender(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetOrganization(int? id);

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
        Task<Project>? TryGetProjectAsync(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetStage(int? id);

        /// <summary>
        /// Try get substrate by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetSubstrate(int? id);

        /// <summary>
        /// Try get summary
        /// </summary>
        /// <param name="source"></param>
        /// <param name="FreeTextSummary"></param>
        /// <returns></returns>
        string? TryGetSummary(string? source, bool FreeTextSummary);

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
        Metadata<int>? TryGetUnit(int? id);

        /// <summary>
        /// Try to get a xxx by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Metadata<int>? TryGetValidationStatus(int? id);
    }
}