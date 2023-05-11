using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.Services.Interfaces
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