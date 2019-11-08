using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    /// <summary>
    /// Metadata repository interface
    /// </summary>
    public interface IMetadataRepository
    {
        /// <summary>
        /// Get all activities
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataWithCategoryEntity>> GetActivitiesAsync();

        /// <summary>
        /// Get all genders
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity>> GetGendersAsync();

        /// <summary>
        /// Get all stages
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity>> GetStagesAsync();

        /// <summary>
        /// Get all units
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity>> GetUnitsAsync();

        /// <summary>
        /// Get all validation status items
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MetadataEntity>> GetValidationStatusAsync();
    }
}
