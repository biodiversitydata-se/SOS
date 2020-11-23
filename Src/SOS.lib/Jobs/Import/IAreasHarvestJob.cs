using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IAreasHarvestJob
    {
        /// <summary>
        ///     Run geo harvest
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest areas from Artportalen db")]
        Task<bool> RunAsync();
    }
}