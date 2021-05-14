using System.Threading.Tasks;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Filter manager.
    /// </summary>
    public interface IFilterManager
    {
        /// <summary>
        /// Creates a with additional information if necessary. E.g. adding underlying taxon ids.
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="authorizationUsePointAccuracy"></param>
        /// <param name="authorizationUseDisturbanceRadius"></param>
        /// <returns></returns>
        Task PrepareFilter(string authorizationApplicationIdentifier, FilterBase filter, int? areaBuffer = 0, bool? authorizationUsePointAccuracy = false, bool? authorizationUseDisturbanceRadius = false);
    }
}
