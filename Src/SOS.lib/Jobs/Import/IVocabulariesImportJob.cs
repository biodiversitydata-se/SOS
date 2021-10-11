using System.ComponentModel;
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
        [DisplayName("Harvest vocabularies from files")]
        [Queue("high")]
        Task<bool> RunAsync();
    }
}