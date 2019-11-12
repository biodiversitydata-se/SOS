using System.Threading.Tasks;

namespace SOS.Import.Jobs.Interfaces
{
    public interface ITaxonHarvestJob
    {
        /// <summary>
        /// Run taxon harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> Run();
    }
}
