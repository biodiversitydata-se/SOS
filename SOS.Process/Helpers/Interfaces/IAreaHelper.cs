using System.Collections.Generic;
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
        void AddAreaDataToDarwinCore(IEnumerable<DarwinCore<DynamicProperties>> darwinCoreModels);
    }
}
