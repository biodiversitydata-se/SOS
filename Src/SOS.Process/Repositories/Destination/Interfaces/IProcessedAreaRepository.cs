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
        Task<IEnumerable<Area>> GetAllExceptGeometryFieldAsync();
    }
}
