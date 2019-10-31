using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IAreaHelper
    {
        /// <summary>
        /// Add area data to darwin core models 
        /// </summary>
        /// <param name="darwinCoreModels"></param>
        /// <returns></returns>
        Task AddAreaDataToDarwinCoreAsync(IEnumerable<DarwinCore<DynamicProperties>> darwinCoreModels);
    }
}
