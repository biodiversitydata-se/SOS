using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAreaRepository : IBaseRepository<Area, int>
    {
        /// <summary>
        /// Get process information
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public Task<Area> GetAreaAsync(int areaId);
        public Task<InternalAreas> GetAllPagedAsync(int skip, int take);
    }
}
