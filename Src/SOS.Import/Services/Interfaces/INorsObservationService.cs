using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface INorsObservationService
    {
        /// <summary>
        ///     Get Nors observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(long changeId);
    }
}