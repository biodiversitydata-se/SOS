using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Area cache
    /// </summary>
    public class AreaCache : CacheBase<int, Area>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        public AreaCache(IAreaRepository areaRepository) : base(areaRepository)
        {

        }
    }
}
