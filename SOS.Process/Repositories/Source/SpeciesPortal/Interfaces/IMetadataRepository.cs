using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Process.Entities;

namespace SOS.Process.Repositories.Source.SpeciesPortal.Interfaces
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
        Task<IEnumerable<MetadataEntity>> GetActivitiesAsync();

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
    }
}
