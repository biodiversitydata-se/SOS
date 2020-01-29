using System.Threading.Tasks;

namespace SOS.Import.Jobs.Interfaces
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
