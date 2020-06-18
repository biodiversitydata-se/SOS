using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.UserService;

namespace SOS.Observations.Api.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseModel<IEnumerable<RoleModel>>> GetUserRolesAsync(int userId);
    }
}
