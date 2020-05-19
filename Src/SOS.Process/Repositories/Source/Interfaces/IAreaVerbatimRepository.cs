using System.Threading.Tasks;
using Nest;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IAreaVerbatimRepository : IVerbatimBaseRepository<Area, int>
    {
        /// <summary>
        /// Get the geometry for a area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        Task<IGeoShape> GetGeometryAsync(int areaId);
    }
}
