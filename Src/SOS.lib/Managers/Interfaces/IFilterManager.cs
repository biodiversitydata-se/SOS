using System.Threading.Tasks;
using SOS.Lib.Models.Search;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Filter manager.
    /// </summary>
    public interface IFilterManager
    {
        /// <summary>
        /// The user service to use when getting authorities.
        /// </summary>
        public IUserService UserService { get; set; }

        /// <summary>
        /// Creates a with additional information if necessary. E.g. adding underlying taxon ids.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="authorityIdentity"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="authorizationUsePointAccuracy"></param>
        /// <param name="authorizationUseDisturbanceRadius"></param>
        /// <param name="setDefaultProviders"></param>
        /// <returns></returns>
        Task PrepareFilter(
            int? roleId,
            string authorizationApplicationIdentifier, 
            FilterBase filter,
            string authorityIdentity = "Sighting",
            int? areaBuffer = 0, 
            bool? authorizationUsePointAccuracy = false, 
            bool? authorizationUseDisturbanceRadius = false, 
            bool? setDefaultProviders = true);
    }
}
