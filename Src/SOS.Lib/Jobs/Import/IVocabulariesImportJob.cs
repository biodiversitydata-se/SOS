using Hangfire;
using System.Threading.Tasks;

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