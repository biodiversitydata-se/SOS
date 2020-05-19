using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IAreaDiffHelper
    {
        /// <summary>
        /// Checks for differences between generated, verbatim and processed areas
        /// and returns the result in a zip file.
        /// </summary>
        /// <returns></returns>
        Task<byte[]> CreateDiffZipFile(Area[] generatedAreas);
    }
}
