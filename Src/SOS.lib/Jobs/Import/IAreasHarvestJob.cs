using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IAreasHarvestJob
    {
        /// <summary>
        ///     Run geo harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}