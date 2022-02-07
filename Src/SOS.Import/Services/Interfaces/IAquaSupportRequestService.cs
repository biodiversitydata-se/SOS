using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface IAquaSupportRequestService
    {
        /// <summary>
        /// Get data from AquaSupport
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="changeId"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(string baseUrl, DateTime startDate, DateTime endDate, long changeId, int maxReturnedChanges = 10000);
    }
}
