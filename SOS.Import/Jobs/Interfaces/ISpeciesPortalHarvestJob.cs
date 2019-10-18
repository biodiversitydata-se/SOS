using System.Threading.Tasks;

namespace SOS.Import.Jobs.Interfaces
{
    public interface ISpeciesPortalHarvestJob
    {
        /// <summary>
        /// Run species portal harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> Run();
    }
}
