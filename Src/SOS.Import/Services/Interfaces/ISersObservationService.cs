using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface ISersObservationService
    {
        /// <summary>
        /// Get SERS observations from specified id.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="changeId"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId);
    }
}