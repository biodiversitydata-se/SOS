using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface ITaxonHarvestJob
    {
        /// <summary>
        /// Run taxon harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
