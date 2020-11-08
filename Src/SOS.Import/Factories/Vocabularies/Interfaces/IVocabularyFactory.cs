using System.Threading.Tasks;

namespace SOS.Import.Factories.Vocabularies.Interfaces
{
    /// <summary>
    ///     Interface for creating field mapping.
    /// </summary>
    public interface IVocabularyFactory
    {
        /// <summary>
        ///     Create field mapping.
        /// </summary>
        /// <returns></returns>
        Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync();
    }
}