using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Filter manager.
    /// </summary>
    public interface IFilterManager
    {
        /// <summary>
        /// Creates a cloned filter with additional information if necessary. E.g. adding underlying taxon ids.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>A cloned filter with additional information.</returns>
        Task<SearchFilter> PrepareFilter(SearchFilter filter);
    }
}
