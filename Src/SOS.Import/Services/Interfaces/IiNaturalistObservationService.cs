using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface IiNaturalistObservationService
    {
        /// <summary>
        ///  Get iNaturalist observations 
        /// </summary>
        /// <param name="changeId"></param>
        /// <returns></returns>
        Task<GBIFResult> GetAsync(DateTime fromDate, DateTime toDate);
    }
}