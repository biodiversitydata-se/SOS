using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IVocabulariesImportJob
    {
        /// <summary>
        ///     Run vocabularies import.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest vocabularies from files")]
        Task<bool> RunAsync();
    }
}