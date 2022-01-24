using System.Threading.Tasks;
using SOS.Lib.Models.ApiManagement;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    /// Interface for Api management 
    /// </summary>
    public interface IApiManagementUserService
    {
        /// <summary>
        /// Get user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiManagementUser> GetUserAsync(string id);
    }
}