using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public interface IAreaProcessedRepository : IProcessBaseRepository<Area, int>
    {
        /// <summary>
        /// Gets all processed areas.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Area>> GetAreasAsync();
    }
}
