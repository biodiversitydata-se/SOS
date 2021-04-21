using SOS.Lib.Models.Verbatim.DarwinCore;
using System;
using System.Collections.Generic;
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
        Task<IEnumerable<DwcObservationVerbatim>> GetAsync(DateTime fromDate, DateTime toDate);
    }
}