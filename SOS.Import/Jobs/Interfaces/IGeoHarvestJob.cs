using System.Threading.Tasks;

namespace SOS.Import.Jobs.Interfaces
{
    public interface IGeoHarvestJob
    {
        /// <summary>
        /// Run geo harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> Run();
    }
}
