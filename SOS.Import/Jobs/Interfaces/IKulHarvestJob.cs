using System.Threading.Tasks;

namespace SOS.Import.Jobs.Interfaces
{
    public interface IKulHarvestJob
    {
        /// <summary>
        /// Run KUL harvest.
        /// </summary>
        /// <returns></returns>
        Task<bool> Run();
    }
}
