using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface IFishDataObservationService
    {
        /// <summary>
        ///  Get KUL observations 
        /// </summary>
        /// <param name="changeId"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(long changeId);
    }
}