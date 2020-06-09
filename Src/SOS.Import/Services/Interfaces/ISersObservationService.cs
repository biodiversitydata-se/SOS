using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface ISersObservationService
    {
        /// <summary>
        ///     Get Nors observations between changedFrom and changedTo.
        /// </summary>
        /// <param name="changedFrom">From date.</param>
        /// <param name="changedTo">To date.</param>
        /// <returns></returns>
        Task<XDocument> GetAsync(long changeId);
    }
}