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
        /// <param name="filter"></param>
        /// <returns></returns>
        Task PrepareFilter(FilterBase filter);
    }
}
