using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Process
{
    public interface ICopyFieldMappingsJob
    {
        /// <summary>
        ///     Copy field mappings from verbatim db to process db.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}