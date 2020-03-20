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
    public interface IProcessedAreaRepository : IProcessBaseRepository<Area, int>
    {
        /// <summary>
        /// Get all areas, but skip getting the geometry field.
        /// </summary>
        /// <returns></returns>
        Task<List<AreaBase>> GetAllAreaBaseAsync();
    }
}
