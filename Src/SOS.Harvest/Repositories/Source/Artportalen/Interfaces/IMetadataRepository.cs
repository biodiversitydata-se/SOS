using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Metadata repository interface
    /// </summary>
    public interface IMetadataRepository : IBaseRepository<IMetadataRepository>
    {
        /// <summary>
        ///     Get all activities
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataWithCategoryEntity<int>>> GetActivitiesAsync();

        /// <summary>
        ///     Get all bioptopes
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetBiotopesAsync();

        /// <summary>
        ///     Get all genders
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetGendersAsync();

        /// <summary>
        ///     Get all organizations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetOrganizationsAsync();

        /// <summary>
        ///     Get all stages
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetStagesAsync();

        /// <summary>
        ///     Get all substrates
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetSubstratesAsync();

        /// <summary>
        ///     Get all units
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetUnitsAsync();

        /// <summary>
        ///     Get all validation status items
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetValidationStatusAsync();

        /// <summary>
        ///     Gets all area types
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetAreaTypesAsync();
        
        /// <summary>
        ///     Gets all discovery methods
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetDiscoveryMethodsAsync();

        /// <summary>
        ///     Gets all determination methods
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<int>>> GetDeterminationMethodsAsync();

        /// <summary>
        /// Get resources
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity<string>>> GetResourcesAsync(string prefix);

        /// <summary>
        /// Get date backup was taken
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> GetLastBackupDateAsync();
    }
}