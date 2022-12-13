using SOS.Observations.Api.Models.DevOps;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Services.Interfaces
{
    /// <summary>
    ///     Interface for taxon list service
    /// </summary>
    public interface IDevOpsService
    {
        /// <summary>
        /// Get releases
        /// </summary>
        /// <param name="definitionId"></param>
        /// <returns></returns>
        Task<MultiResponse<ReleaseDefinition>> GetReleasesAsync(int definitionId);

        /// <summary>
        /// Get a release by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Release> GetReleaseAsync(int id);
    }
}