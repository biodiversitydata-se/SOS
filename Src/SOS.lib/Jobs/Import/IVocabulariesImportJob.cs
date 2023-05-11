using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IVocabulariesImportJob
    {
        /// <summary>
        ///     Run vocabularies import.
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("Harvest vocabularies from files")]
        [Queue("high")]
        Task<bool> RunAsync();
    }
}