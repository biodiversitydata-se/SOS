using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IGeoHarvestJob
    {
        /// <summary>
        /// Run geo harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
