using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IFieldMappingImportJob
    {
        /// <summary>
        /// Run field mapping import.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
