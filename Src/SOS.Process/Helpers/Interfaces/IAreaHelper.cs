using System.Collections.Generic;
using SOS.Lib.Models.Processed.DarwinCore;

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

        /// <summary>
        /// Save cache so we can use it after restart
        /// </summary>
        void PersistCache();
    }
}
