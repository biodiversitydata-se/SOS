using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Harvest.Services.Interfaces
{
    public interface IVirtualHerbariumObservationService
    {
        /// <summary>
        ///     Get observations
        /// </summary>
        /// <param name="from"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(DateTime from, int pageIndex, int pageSize);

        /// <summary>
        ///     Get list of localities
        /// </summary>
        /// <returns></returns>
        Task<XDocument> GetLocalitiesAsync();
    }
}