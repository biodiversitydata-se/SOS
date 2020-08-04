using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Search;

namespace SOS.Export.Managers.Interfaces
{
    public interface IFilterManager
    {
        /// <summary>
        /// Creates a cloned filter with additional information if necessary. E.g. adding underlying taxon ids.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>A cloned filter with additional information.</returns>
        Task<ExportFilter> PrepareFilter(ExportFilter filter);
    }
}
