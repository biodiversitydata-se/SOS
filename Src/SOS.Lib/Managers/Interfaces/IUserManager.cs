using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     User manager interface.
    /// </summary>
    public interface IUserManager
    {
        IUserService UserService { get; set; }
        Task<UserInformation> GetUserInformationAsync(string applicationIdentifier, string cultureCode);
        Task<UserInformation> GetBasicUserInformationAsync();
    }
}